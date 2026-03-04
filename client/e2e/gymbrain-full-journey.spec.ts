import { test, expect } from '@playwright/test';

/**
 * 🧠 GymBrain — Full End-to-End Journey
 *
 * This E2E test validates the complete user flow:
 *   1. Register → 2. Vault API Key → 3. Generate Workout → 4. Reset → 5. Sign Out
 */

const TEST_ID = Date.now();
const TEST_EMAIL = `e2e_pw_${TEST_ID}@gymbrain.com`;
const TEST_PASSWORD = 'Test1234!';

// Groq API key from environment (set via CI or locally)
const GROQ_API_KEY = process.env.GROQ_API_KEY || 'gsk_test_placeholder';

test.describe.serial('🧠 GymBrain Full E2E Journey', () => {

    test.setTimeout(120_000); // 2 min per test

    // ─────────────────────────────
    // 1️⃣  REGISTER
    // ─────────────────────────────
    test('1️⃣ Register a new user account', async ({ page }) => {
        await page.goto('/');

        // Should see the auth page with "GymBrain"
        await expect(page.getByText('GymBrain')).toBeVisible();

        // Switch to Sign Up mode
        const signUpBtn = page.getByText("Don't have an account? Sign up");
        if (await signUpBtn.isVisible()) {
            await signUpBtn.click();
        }

        // Fill registration form
        await page.fill('#email', TEST_EMAIL);
        await page.fill('#password', TEST_PASSWORD);

        // Select Hype Bro persona (for fun 🔥)
        const personaSelect = page.locator('#persona');
        if (await personaSelect.isVisible()) {
            await personaSelect.selectOption('Hype Bro');
        }

        // Screenshot: Registration form filled
        await page.screenshot({ path: 'e2e/screenshots/01-register-form.png' });

        // Submit registration
        await page.getByRole('button', { name: /create account/i }).click();

        // Wait for the Vault page to appear (react state transition)
        await expect(page.getByText('Vault Setup')).toBeVisible({ timeout: 10_000 });

        // Screenshot: Vault page after registration
        await page.screenshot({ path: 'e2e/screenshots/02-vault-page.png' });

        console.log(`✅ Registration success: ${TEST_EMAIL}`);
    });

    // ─────────────────────────────
    // 2️⃣  VAULT GROQ API KEY
    // ─────────────────────────────
    test('2️⃣ Vault Groq API key with health check', async ({ page }) => {
        // Login (each test gets fresh browser context)
        await page.goto('/');
        await page.fill('#email', TEST_EMAIL);
        await page.fill('#password', TEST_PASSWORD);
        await page.getByRole('button', { name: /sign in/i }).click();

        // Wait for Vault page (new users go to vault first)
        await expect(page.getByText('Vault Setup')).toBeVisible({ timeout: 10_000 });

        // Select Groq provider
        await page.locator('#provider').selectOption('groq');

        // Verify the model dropdown updated to show Groq models
        await page.waitForTimeout(500); // let React re-render

        // Select Llama 3.3 70B
        const modelSelect = page.locator('#model');
        await modelSelect.selectOption({ value: 'llama-3.3-70b-versatile' });

        // Screenshot: Groq + Llama selected
        await page.screenshot({ path: 'e2e/screenshots/03-groq-selected.png' });

        // Enter API key
        await page.fill('#apikey', GROQ_API_KEY);

        // Click Vault My Key
        await page.getByRole('button', { name: /vault my key/i }).click();

        // Should show "Encrypting & Storing..." loading state
        await expect(page.getByText('Encrypting & Storing...')).toBeVisible({ timeout: 3_000 });

        // Wait for health check + vaulting to complete
        // After vault, app now navigates to Home page (tab-based navigation)
        await expect(page.getByText('Hello,')).toBeVisible({ timeout: 30_000 });

        // Screenshot: Home page (health check passed!)
        await page.screenshot({ path: 'e2e/screenshots/04-home-page.png' });

        console.log('✅ Health check passed, key vaulted successfully!');
    });

    // ─────────────────────────────
    // 3️⃣  GENERATE WORKOUT
    // ─────────────────────────────
    test('3️⃣ Generate AI workout and verify exercise cards', async ({ page }) => {
        // Login
        await page.goto('/');
        await page.fill('#email', TEST_EMAIL);
        await page.fill('#password', TEST_PASSWORD);
        await page.getByRole('button', { name: /sign in/i }).click();

        // Since key is already vaulted, we may land on Vault or Home
        await page.waitForTimeout(2_000);

        // If we're on vault, skip to home
        const skipBtn = page.getByText('Skip for now');
        if (await skipBtn.isVisible()) {
            await skipBtn.click();
        }

        // Wait for either Home or Ready to Train
        await Promise.race([
            expect(page.getByText('Hello,')).toBeVisible({ timeout: 10_000 }),
            expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 10_000 }),
        ]);

        // If on Home, navigate to Train tab
        const trainTab = page.locator('.bottom-nav__item', { hasText: 'Train' });
        if (await trainTab.isVisible()) {
            await trainTab.click();
            await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 5_000 });
        }

        // Now we're on the workout/train page
        await expect(page.getByText('Ready to Train')).toBeVisible();

        // Select "Chest & Arms" for this test
        await page.locator('#focus').selectOption('chest and arms');

        // Screenshot: Before generation
        await page.screenshot({ path: 'e2e/screenshots/05-before-generate.png' });

        // Click Generate
        await page.getByRole('button', { name: /generate workout/i }).click();

        // Should show loading spinner
        await expect(page.getByText('Generating your workout with AI...')).toBeVisible({ timeout: 5_000 });

        // Screenshot: Loading spinner
        await page.screenshot({ path: 'e2e/screenshots/06-loading-spinner.png' });

        // Wait for LLM response (can take up to 30-40s on cold start)
        await expect(page.getByText('Your Workout')).toBeVisible({ timeout: 60_000 });

        // Give React a moment to render all components
        await page.waitForTimeout(1_000);

        // Screenshot: Full workout result
        await page.screenshot({ path: 'e2e/screenshots/07-workout-generated.png', fullPage: true });

        // ─── ASSERTIONS ───

        // 1. "Your Workout" header
        await expect(page.getByText('Your Workout')).toBeVisible();

        // 2. "AI-generated • SafetyGate validated" subtitle  
        await expect(page.getByText(/SafetyGate validated/i)).toBeVisible();

        // 3. At least 1 tone_card (motivational message in quotes)
        const toneCards = page.locator('.tone-card');
        const toneCount = await toneCards.count();
        expect(toneCount).toBeGreaterThanOrEqual(1);
        const motivationalText = await toneCards.first().textContent();
        console.log(`  📣 Tone Card: ${motivationalText}`);

        // 4. At least 3 exercise cards
        const exerciseCards = page.locator('.exercise-card');
        const exerciseCount = await exerciseCards.count();
        expect(exerciseCount).toBeGreaterThanOrEqual(3);

        // 5. Each exercise has name
        for (let i = 0; i < exerciseCount; i++) {
            const card = exerciseCards.nth(i);
            const name = await card.locator('.exercise-name').textContent();
            expect(name).toBeTruthy();
            console.log(`  💪 ${name}`);
        }

        // 6. NO "No workout components" error
        await expect(page.getByText('No workout components')).not.toBeVisible();

        // 7. Save + New buttons are visible
        await expect(page.getByRole('button', { name: /save/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /new/i })).toBeVisible();

        console.log(`\n✅ Workout generated with ${toneCount} tone card(s) and ${exerciseCount} exercise(s)!`);
    });

    // ─── Helper: Login → Skip Vault → Navigate to Train ───
    async function loginToTrainPage(page: import('@playwright/test').Page) {
        await page.goto('/');
        await page.fill('#email', TEST_EMAIL);
        await page.fill('#password', TEST_PASSWORD);
        await page.getByRole('button', { name: /sign in/i }).click();

        // Wait for navigation
        await page.waitForTimeout(2_000);

        // If we landed on Vault, skip to home
        const skipBtn = page.getByText('Skip for now');
        if (await skipBtn.isVisible()) {
            await skipBtn.click();
        }

        // Wait for Home or Train
        await Promise.race([
            expect(page.getByText('Hello,')).toBeVisible({ timeout: 10_000 }),
            expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 10_000 }),
        ]);

        // Navigate to Train tab if on Home
        const trainTab = page.locator('.bottom-nav__item', { hasText: 'Train' });
        if (await trainTab.isVisible() && !(await page.getByText('Ready to Train').isVisible())) {
            await trainTab.click();
        }

        await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 10_000 });
    }

    // ─────────────────────────────
    // 4️⃣  RESET & RE-GENERATE
    // ─────────────────────────────
    test('4️⃣ Reset workout and return to ready state', async ({ page }) => {
        await loginToTrainPage(page);

        // Generate a workout 
        await page.getByRole('button', { name: /generate workout/i }).click();
        await expect(page.getByText('Your Workout')).toBeVisible({ timeout: 60_000 });

        // Click "New" button to reset
        await page.getByRole('button', { name: /new/i }).click();

        // Should be back to "Ready to Train"
        await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 5_000 });
        await expect(page.locator('#focus')).toBeVisible();

        // Screenshot: Back to ready state
        await page.screenshot({ path: 'e2e/screenshots/08-back-to-ready.png' });

        console.log('✅ Reset successful, back to Ready to Train!');
    });

    // ─────────────────────────────
    // 5️⃣  SIGN OUT
    // ─────────────────────────────
    test('5️⃣ Sign out returns to login screen', async ({ page }) => {
        await loginToTrainPage(page);

        // Navigate to Profile tab (Sign Out is now in Profile)
        const profileTab = page.locator('.bottom-nav__item', { hasText: 'Profile' });
        await profileTab.click();
        await expect(page.getByText('Your Profile')).toBeVisible({ timeout: 5_000 });

        // Click Sign Out
        await page.getByRole('button', { name: /sign out/i }).click();

        // Should see login screen again
        await expect(page.getByText('Welcome Back')).toBeVisible({ timeout: 5_000 });
        await expect(page.getByText('GymBrain')).toBeVisible();

        // Screenshot: Logged out
        await page.screenshot({ path: 'e2e/screenshots/09-signed-out.png' });

        console.log('✅ Sign out successful!');
    });
});
