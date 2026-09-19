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
    if (path === '/api/profile') return route.fulfill({ json: profile.name ? serverProfile : { ...serverProfile, goal: null } });
    if (path === '/api/workout/history') return route.fulfill({ json: { items: [], total: 0, hasMore: false } });
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
  await page.route('**/api/auth/models/discover', route => route.fulfill({ json: [{ provider: 'groq', modelId: 'fresh-model', displayName: 'Fresh model' }] }));
  await page.getByLabel('API Key', { exact: true }).fill('test-provider-secret');
  await page.getByRole('button', { name: 'Load latest models' }).click();
  await expect(page.getByLabel('Model', { exact: true })).toHaveValue('fresh-model');
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

test('completion retry retains actual results and history survives a fresh browser session', async ({ page, browser }) => {
  await signInLocally(page, { name: 'Athlete' });
  await page.route('**/api/workout/start', route => route.fulfill({ json: { megaPayloadJson: JSON.stringify({
    components: [{ type: 'set_tracker', payload: { exercise_id: 'squat', exercise_name: 'Squat', sets: 2, reps: 10 } }],
  }) } }));
  await page.route('**/api/workout/exercise-metadata/**', route => route.fulfill({ status: 204 }));
  const writes: { payloadJson: string; sessionId: string }[] = [];
  await page.route('**/api/workout/save', route => {
    writes.push(route.request().postDataJSON());
    return route.fulfill({ status: writes.length === 1 ? 503 : 200,
      json: writes.length === 1 ? { detail: 'Connection interrupted' } : { workoutSessionId: writes[0].sessionId, unlockedMilestones: [] } });
  });
  await page.goto('/');
  await page.getByRole('button', { name: /Start Training/ }).click();
  await page.getByRole('button', { name: /Generate Workout/ }).click();
  await page.getByLabel('Squat set 1 reps', { exact: true }).fill('8');
  await page.getByLabel('Squat set 1 weight kg', { exact: true }).fill('25');
  await page.getByTitle('Set 1', { exact: true }).click();
  await page.reload();
  await page.getByRole('button', { name: /Start Training/ }).click();
  await page.getByRole('button', { name: /Resume Workout/ }).click();
  await expect(page.getByLabel('Squat set 1 reps', { exact: true })).toHaveValue('8');
  await expect(page.getByTitle('Set 1', { exact: true })).toHaveClass(/set-circle--done/);
  await page.getByRole('button', { name: 'Finish & Save' }).click();
  await expect(page.getByRole('alert')).toContainText('Connection interrupted');
  await expect(page.getByLabel('Squat set 1 reps', { exact: true })).toBeDisabled();
  await page.getByRole('button', { name: 'Retry save' }).click();
  await expect(page.getByRole('button', { name: 'Saved', exact: true })).toBeDisabled();
  expect(writes).toHaveLength(2);
  expect(writes[1]).toEqual(writes[0]);
  expect(JSON.parse(writes[0].payloadJson).exercises[0].sets[0]).toEqual({ completed: true, reps: 8, weightKg: 25 });

  const context = await browser.newContext();
  const fresh = await context.newPage();
  // No local profile or history on this device: only auth and the server response.
  await signInLocally(fresh);
  await fresh.route('**/api/profile', route => route.fulfill({ json: serverProfile }));
  await fresh.route('**/api/workout/history?*', route => route.fulfill({ json: {
    items: [{ id: writes[0].sessionId, completedAtUtc: new Date().toISOString(), payloadJson: writes[0].payloadJson }], total: 1, hasMore: false,
  } }));
  await fresh.goto('http://127.0.0.1:5178/');
  await expect(fresh.getByText('Hello,', { exact: true })).toBeVisible();
  await fresh.getByRole('button', { name: /History/ }).click();
  await expect(fresh.getByText('1 saved sessions', { exact: true })).toBeVisible();
  await fresh.locator('summary').click();
  await expect(fresh.getByText('Set 1: 8 reps × 25 kg', { exact: true })).toBeVisible();
  await context.close();
});

test('failed profile load blocks editing and retry restores server preferences', async ({ page }) => {
  await signInLocally(page, { name: 'Returning Athlete' });
  await page.goto('/');
  await expect(page.getByText('Hello,', { exact: true })).toBeVisible();
  let fail = true;
  let writes = 0;
  await page.route('**/api/profile', route => route.fulfill(fail
    ? { status: 503, json: { detail: 'Temporarily unavailable' } }
    : { json: serverProfile }));
  await page.route('**/api/profile/save', route => {
    writes++;
    expect(route.request().postDataJSON().experienceLevel).toBe('Advanced');
    return route.fulfill({ json: { message: 'Saved' } });
  });
  await page.getByRole('button', { name: /Profile/ }).click();
  await expect(page.getByRole('alert')).toContainText('Could not load your saved profile');
  await expect(page.getByRole('button', { name: /Save Profile/ })).toHaveCount(0);
  expect(writes).toBe(0);
  fail = false;
  await page.getByRole('button', { name: 'Retry loading profile' }).click();
  await expect(page.getByRole('button', { name: 'Advanced', exact: true })).toHaveClass(/active/);
  await page.getByRole('button', { name: /Save Profile/ }).click();
  await expect.poll(() => writes).toBe(1);
});

test('legacy workouts are not counted as zero-result completed sessions', async ({ page }) => {
  await signInLocally(page, { name: 'Athlete' });
  await page.route('**/api/workout/history?*', route => route.fulfill({ json: {
    items: [
      { id: 'legacy', completedAtUtc: '2026-09-01T12:00:00Z', payloadJson: '{}' },
      { id: 'recorded', completedAtUtc: '2026-09-02T12:00:00Z', payloadJson: JSON.stringify({
        schemaVersion: 1, focus: 'Strength', exercises: [{ name: 'Squat', sets: [
          { completed: true, reps: 8, weightKg: 20 }, { completed: true, reps: 8, weightKg: 20 },
        ] }],
      }) },
    ], total: 2, hasMore: false,
  } }));
  await page.goto('/');
  await expect(page.getByText('Older workout · set results unavailable', { exact: true })).toBeVisible();
  await expect(page.getByText('Avg Sets/Workout: 2', { exact: true })).toBeVisible();
  await expect(page.getByText('1 exercises · 2/2 sets', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: /History/ }).click();
  await page.locator('summary').first().click();
  await expect(page.getByText('Saved with an earlier version. Actual set results were not recorded.')).toBeVisible();
});

test('Vault discovers live models, clears stale choices and keeps provider keys out of storage', async ({ page }) => {
  await signInLocally(page, { name: 'Athlete' });
  let calls = 0;
  await page.route('**/api/auth/models/discover', route => {
    calls++;
    return route.fulfill(calls === 1 ? { status: 400, json: { detail: 'Provider is rate limiting requests. Retry later.' } } :
      { json: [{ provider: 'openrouter', modelId: 'new/model:free', displayName: 'New model', isFree: true }] });
  });
  await page.route('**/api/auth/vault-key', route => {
    expect(route.request().postDataJSON().model).toBe('new/model:free');
    return route.fulfill({ json: { message: 'Connection saved' } });
  });
  await page.goto('/'); await page.getByRole('button', { name: /Vault/ }).click();
  await page.getByLabel('API key', { exact: true }).fill('test-key-not-real');
  await page.getByRole('button', { name: 'Load latest models' }).click();
  await expect(page.getByRole('alert')).toContainText('rate limiting');
  await expect(page.getByRole('button', { name: 'Save connection' })).toBeDisabled();
  await page.getByRole('button', { name: 'Load latest models' }).click();
  await expect(page.getByLabel('Model', { exact: true })).toHaveValue('new/model:free');
  await page.getByRole('button', { name: 'Save connection' }).click();
  await expect(page.getByText('Connection saved', { exact: true })).toBeVisible();
  await expect(page.getByLabel('API key', { exact: true })).toHaveValue('');
  expect(await page.evaluate(() => JSON.stringify(localStorage))).not.toContain('test-key-not-real');
});

test('meal plan shows one recipe at a time with ingredients and cooking steps', async ({ page }) => {
  await signInLocally(page, { name: 'Athlete' });
  await page.route('**/api/nutrition/generate', route => route.fulfill({ json: { payloadJson: JSON.stringify({ days: [
    { day_number: 1, meals: [{ name: 'Oat bowl', type: 'Breakfast', ingredients: [{ name: 'Oats', quantity: '50 g' }], steps: ['Simmer oats in water.'], servings: 1, prep_minutes: 2, cook_minutes: 5 }] },
    { day_number: 2, meals: [{ name: 'Lentil soup', type: 'Lunch', ingredients: [{ name: 'Lentils', quantity: '100 g' }], steps: ['Rinse lentils.', 'Simmer until tender.'] }] },
  ] }) } }));
  await page.goto('/'); await page.getByRole('button', { name: /Profile/ }).click();
  await page.getByRole('button', { name: /Generate AI Meal Plan/ }).click();
  await expect(page.getByRole('heading', { name: 'Oat bowl' })).toBeVisible();
  await expect(page.getByText('50 g', { exact: true })).toBeVisible();
  await expect(page.getByText('Simmer oats in water.')).toBeVisible();
  await page.getByRole('article').screenshot({ path: test.info().outputPath('meal-card.png') });
  await expect(page.getByRole('heading', { name: 'Lentil soup' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Previous meal' })).toBeDisabled();
  await page.getByRole('button', { name: 'Next meal' }).click();
  await expect(page.getByRole('heading', { name: 'Lentil soup' })).toBeVisible();
  await expect(page.getByText('Day 2 · Lunch')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Next meal' })).toBeDisabled();
  await page.getByRole('button', { name: 'Previous meal' }).click();
  await expect(page.getByRole('heading', { name: 'Oat bowl' })).toBeVisible();
});

test('personal profile comes from the server on a fresh browser and ignores stale local values', async ({ page, browser }) => {
  const personalProfile = { name: 'Server Athlete', age: 34, height: 180, weight: 80, focusAreas: ['Core'] };
  let saved = personalProfile;
  await signInLocally(page, { name: 'Stale local name', age: 20, height: 150, weight: 50, focusAreas: ['Arms'] });
  await page.route('**/api/profile', route => route.fulfill({ json: { ...serverProfile, personalProfile: saved } }));
  await page.route('**/api/profile/save', route => {
    saved = route.request().postDataJSON().personalProfile;
    return route.fulfill({ json: { message: 'Saved' } });
  });
  await page.goto('/');
  await expect(page.getByText('Server Athlete 👋', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: /Profile/ }).click();
  await expect(page.getByLabel('Name', { exact: true })).toHaveValue('Server Athlete');
  await expect(page.getByRole('button', { name: '✓ Core', exact: true })).toHaveClass(/selected/);
  await page.getByLabel('Name', { exact: true }).fill('Updated Athlete');
  await page.getByRole('button', { name: /Save Profile/ }).click();
  await expect.poll(() => saved.name).toBe('Updated Athlete');
  expect(saved).toEqual({ ...personalProfile, name: 'Updated Athlete' });
  const context = await browser.newContext();
  const fresh = await context.newPage();
  await signInLocally(fresh);
  await fresh.route('**/api/profile', route => route.fulfill({ json: { ...serverProfile, personalProfile: saved } }));
  await fresh.goto('http://127.0.0.1:5178/');
  await expect(fresh.getByText('Updated Athlete 👋', { exact: true })).toBeVisible();
  await fresh.getByRole('button', { name: /Profile/ }).click();
  await expect(fresh.getByLabel('Name', { exact: true })).toHaveValue('Updated Athlete');
  await expect(fresh.getByText('34 years', { exact: true })).toBeVisible();
  await expect(fresh.getByText('180 cm', { exact: true })).toBeVisible();
  await expect(fresh.getByText('80 kg', { exact: true })).toBeVisible();
  await context.close();
});
