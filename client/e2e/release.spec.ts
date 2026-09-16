import { test, expect, type Page } from '@playwright/test';

const serverProfile = {
  goal: 'muscle', equipmentJson: '["Dumbbells"]', injuries: '', daysPerWeek: 4,
  dietaryPreference: 'Standard', dailyCalories: 2000, experienceLevel: 'Advanced', workoutsCompleted: 2,
};

async function signInLocally(page: Page, profile: Record<string, unknown> = {}) {
  await page.addInitScript(profile => {
    localStorage.setItem('gymbrain_token', 'test-token');
    localStorage.setItem('gymbrain_userId', 'test-user');
    localStorage.setItem('gymbrain_email', 'test@example.invalid');
    localStorage.setItem('gymbrain_profile', JSON.stringify(profile));
  }, profile);
  // Every API call is intercepted: these UI checks never touch production data/providers.
  await page.route('**/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/auth/models') return route.fulfill({ json: [] });
    if (path === '/api/profile') return route.fulfill({ json: serverProfile });
    if (path === '/api/events') return route.fulfill({ json: {} });
    return route.fulfill({ status: 503, json: { detail: 'Unconfigured test API request' } });
  });
}

async function reachFinalSetup(page: Page) {
  await page.goto('/');
  await page.getByLabel('Your Name').fill('Test Athlete');
  await page.getByRole('button', { name: /Continue/ }).click();
  await page.getByRole('button', { name: 'Advanced', exact: true }).click();
  await page.getByRole('button', { name: /Continue/ }).click();
}

test('onboarding sends level and remains retryable after cloud save failure', async ({ page }) => {
  await signInLocally(page);
  let attempts = 0;
  const writes: Record<string, unknown>[] = [];
  await page.route('**/api/profile/save', route => {
    writes.push(route.request().postDataJSON());
    attempts++;
    return route.fulfill({ status: attempts === 1 ? 503 : 200,
      json: attempts === 1 ? { detail: 'Please retry setup' } : { message: 'Saved' } });
  });
  await reachFinalSetup(page);
  await page.getByRole('button', { name: /Let's Go/ }).click();
  await expect(page.getByText('Please retry setup')).toBeVisible();
  await expect(page.getByRole('button', { name: /Let's Go/ })).toBeEnabled();
  expect(await page.evaluate(() => JSON.parse(localStorage.getItem('gymbrain_profile') || '{}').name)).toBeUndefined();
  await page.getByRole('button', { name: /Let's Go/ }).click();
  await expect(page.getByText('Hello,', { exact: true })).toBeVisible();
  expect(writes).toHaveLength(2);
  expect(writes[1].experienceLevel).toBe('Advanced');
  expect(writes[1]).not.toHaveProperty('workoutsCompleted');
});

test('BYO onboarding never persists the provider key, including after retry', async ({ page }) => {
  await signInLocally(page);
  await page.route('**/api/auth/vault-key', route => route.fulfill({ json: { message: 'Vaulted' } }));
  let attempts = 0;
  await page.route('**/api/profile/save', route => route.fulfill({ status: ++attempts === 1 ? 503 : 200,
    json: attempts === 1 ? { detail: 'Save unavailable' } : { message: 'Saved' } }));
  await reachFinalSetup(page);
  await page.getByText('BYO API Key', { exact: true }).click();
  await page.getByLabel('API Key', { exact: true }).fill('test-provider-secret');
  await page.getByRole('button', { name: /Let's Go/ }).click();
  await expect(page.getByText('Save unavailable')).toBeVisible();
  await expect(page.getByRole('button', { name: /Let's Go/ })).toBeEnabled();
  await page.getByRole('button', { name: /Let's Go/ }).click();
  await expect(page.getByText('Hello,', { exact: true })).toBeVisible();
  const storage = await page.evaluate(() => JSON.stringify(localStorage));
  expect(storage).not.toContain('test-provider-secret');
  expect(JSON.parse(await page.evaluate(() => localStorage.getItem('gymbrain_profile')!))).not.toHaveProperty('apiKey');
});

test('legacy provider key is removed and server level wins over stale local profile', async ({ page }) => {
  await signInLocally(page, { name: 'Returning Athlete', level: 'Beginner', apiKey: 'old-test-secret' });
  const writes: Record<string, unknown>[] = [];
  await page.route('**/api/profile/save', route => {
    writes.push(route.request().postDataJSON());
    return route.fulfill({ json: { message: 'Saved' } });
  });
  await page.goto('/');
  expect(await page.evaluate(() => localStorage.getItem('gymbrain_profile'))).not.toContain('old-test-secret');
  await page.getByRole('button', { name: /Profile/ }).click();
  await expect(page.getByRole('button', { name: 'Advanced', exact: true })).toHaveClass(/active/);
  await page.getByRole('button', { name: /Save Profile/ }).click();
  await expect.poll(() => writes.length).toBe(1);
  expect(writes[0].experienceLevel).toBe('Advanced');
  expect(writes[0]).not.toHaveProperty('workoutsCompleted');
});

test('invalid optional exercise metadata does not crash workout rendering', async ({ page }) => {
  await signInLocally(page, { name: 'Athlete' });
  await page.route('**/api/workout/start', route => route.fulfill({ json: { megaPayloadJson: JSON.stringify({
    components: [{ type: 'set_tracker', payload: { exercise_id: 'test-exercise', exercise_name: 'Squat', sets: 3, reps: 10 } }],
  }) } }));
  await page.route('**/api/workout/exercise-metadata/**', route => route.fulfill({ json: {
    id: 'test-exercise', name: 'Squat', gifUrl: 'javascript:bad', instructions: 'invalid-shape',
  } }));
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/');
  await page.getByRole('button', { name: /Train/, exact: false }).last().click();
  await page.getByRole('button', { name: /Generate Workout/ }).click();
  await expect(page.locator('.exercise-card')).toHaveCount(1);
  await expect(page.locator('.exercise-card')).toContainText('Squat');
  await expect(page.getByText('Form Tips & Instructions', { exact: false })).toHaveCount(0);
  expect(errors).toEqual([]);
});

test('substituting another exercise preserves completed sets', async ({ page }) => {
  await signInLocally(page, { name: 'Athlete' });
  await page.route('**/api/workout/start', route => route.fulfill({ json: { megaPayloadJson: JSON.stringify({
    components: ['Squat', 'Row'].map((name, index) => ({ type: 'set_tracker', payload: {
      exercise_id: `exercise-${index}`, exercise_name: name, sets: 3, reps: 10,
    } })),
  }) } }));
  await page.route('**/api/workout/exercise-metadata/**', route => route.fulfill({ status: 204 }));
  await page.route('**/api/workout/substitute', route => route.fulfill({ json: {
    substitutes: [{ exerciseId: 'alternative', name: 'Band Row', equipment: 'Bands', reason: 'Alternative equipment' }],
  } }));
  await page.goto('/');
  await page.getByRole('button', { name: /Start Training/ }).click();
  await page.getByRole('button', { name: /Generate Workout/ }).click();
  const first = page.locator('.exercise-card').first();
  await first.getByTitle('Set 1', { exact: true }).click();
  await expect(first.getByTitle('Set 1', { exact: true })).toHaveClass(/set-circle--done/);
  await page.locator('.exercise-card').nth(1).getByRole('button', { name: /Equipment Busy/ }).click();
  await page.getByRole('button', { name: 'Use This' }).click();
  await expect(page.locator('.exercise-card').nth(1)).toContainText('Band Row');
  await expect(first.getByTitle('Set 1', { exact: true })).toHaveClass(/set-circle--done/);
});
