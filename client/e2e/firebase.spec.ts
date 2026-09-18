import { test, expect, type Page } from '@playwright/test';

async function mockFirebase(page: Page, verified = true) {
    const now = Math.floor(Date.now() / 1000);
    const token = [ { alg: 'RS256', typ: 'JWT' }, { sub: 'firebase-user', user_id: 'firebase-user', email: 'athlete@example.invalid',
        email_verified: verified, iat: now, exp: now + 3600, auth_time: now, aud: 'test-project', iss: 'https://securetoken.google.com/test-project',
        firebase: { sign_in_provider: 'password' } } ].map(value => Buffer.from(JSON.stringify(value)).toString('base64url')).join('.') + '.test';
    await page.route('https://identitytoolkit.googleapis.com/**', route => {
        const url = route.request().url();
        if (url.includes('accounts:lookup')) return route.fulfill({ json: { users: [{ localId: 'firebase-user', email: 'athlete@example.invalid', emailVerified: verified,
            providerUserInfo: [{ providerId: 'password', email: 'athlete@example.invalid', rawId: 'athlete@example.invalid' }], createdAt: String(now * 1000), lastLoginAt: String(now * 1000) }] } });
        if (url.includes('accounts:sendOobCode')) return route.fulfill({ json: { email: 'athlete@example.invalid' } });
        if (url.includes('accounts:signInWithPassword')) return route.fulfill({ json: { localId: 'firebase-user', email: 'athlete@example.invalid', idToken: token, refreshToken: 'test-refresh', expiresIn: '3600', registered: true } });
        return route.fulfill({ status: 400, json: { error: { message: 'OPERATION_NOT_ALLOWED' } } });
    });
    await page.route('https://securetoken.googleapis.com/**', route => route.fulfill({ json: { access_token: token, id_token: token, expires_in: '3600', token_type: 'Bearer', refresh_token: 'test-refresh', user_id: 'firebase-user', project_id: 'test-project' } }));
    await page.route('**/api/**', route => {
        const path = new URL(route.request().url()).pathname;
        if (path === '/api/profile') return route.fulfill({ json: { goal: 'strength', experienceLevel: 'Beginner', equipmentJson: '[]', workoutsCompleted: 1 } });
        if (path === '/api/workout/history') return route.fulfill({ json: { items: [], total: 0, hasMore: false } });
        if (path === '/api/auth/models') return route.fulfill({ json: [] });
        return route.fulfill({ status: 503, json: { detail: 'Unexpected test request' } });
    });
}
test('Firebase sign-in requires explicit proof before linking and keeps passwords out of storage', async ({ page }) => {
    await mockFirebase(page);
    let attempts = 0;
    await page.route('**/api/auth/firebase', route => {
        attempts++;
        const password = route.request().postDataJSON().legacyPassword;
        return route.fulfill(password === 'PreviousPassword123!' ? { json: { userId: 'existing-id', email: 'athlete@example.invalid' } } :
            { status: 409, json: { code: 'account_link_required', detail: 'Link required' } });
    });
    await page.goto('/');
    await expect(page.getByRole('button', { name: 'Sign in with Google' })).toBeVisible();
    await page.getByLabel('Email', { exact: true }).fill('athlete@example.invalid');
    await page.getByLabel('Password', { exact: true }).fill('FirebasePassword123!');
    await page.getByRole('button', { name: 'Sign In', exact: true }).click();
    await expect(page.getByLabel('Previous GymBrain password')).toBeVisible();
    expect(await page.evaluate(() => localStorage.getItem('gymbrain_userId'))).toBeNull();
    await page.getByLabel('Previous GymBrain password').fill('PreviousPassword123!');
    await page.getByRole('button', { name: 'Link account and continue' }).click();
    await expect(page.getByText('Hello,', { exact: true })).toBeVisible();
    expect(attempts).toBe(2);
    expect(await page.evaluate(() => localStorage.getItem('gymbrain_userId'))).toBe('existing-id');
    expect(await page.evaluate(() => JSON.stringify(localStorage))).not.toContain('Password123!');
    await page.reload();
    await expect(page.getByText('Hello,', { exact: true })).toBeVisible();
});
test('unverified Firebase email cannot open the app', async ({ page }) => {
    await mockFirebase(page, false);
    let connected = false;
    await page.route('**/api/auth/firebase', route => { connected = true; return route.abort(); });
    await page.goto('/');
    await page.getByLabel('Email', { exact: true }).fill('athlete@example.invalid');
    await page.getByLabel('Password', { exact: true }).fill('FirebasePassword123!');
    await page.getByRole('button', { name: 'Sign In', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Check your email' })).toBeVisible();
    expect(connected).toBe(false);
});
test('recovery gives a neutral response and sends the selected email to Firebase', async ({ page }) => {
    await mockFirebase(page);
    let recipient = '';
    await page.route('https://identitytoolkit.googleapis.com/**/accounts:sendOobCode*', route => {
        recipient = route.request().postDataJSON().email;
        return route.fulfill({ json: { email: recipient } });
    });
    await page.goto('/');
    await page.getByLabel('Email', { exact: true }).fill('athlete@example.invalid');
    await page.getByRole('button', { name: 'Forgot password?' }).click();
    await expect(page.getByRole('status')).toContainText('If this email has a Firebase password account');
    expect(recipient).toBe('athlete@example.invalid');
});

test('blocked Google popup leaves a retryable sign-in screen', async ({ page }) => {
    await mockFirebase(page);
    await page.addInitScript(() => { window.open = () => null; });
    await page.goto('/');
    await page.getByRole('button', { name: 'Sign in with Google' }).click();
    await expect(page.getByRole('alert')).toContainText('Allow popups');
    await expect(page.getByRole('button', { name: 'Sign in with Google' })).toBeEnabled();
    expect(await page.evaluate(() => localStorage.getItem('gymbrain_userId'))).toBeNull();
});
