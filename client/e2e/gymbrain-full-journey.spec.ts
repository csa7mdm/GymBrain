import { test, expect } from '@playwright/test';

// ─────────────────────────────────────────────────────────
//  🧠 GymBrain — Automated E2E Test Suite (Playwright)
// ─────────────────────────────────────────────────────────
//  Tests the full user journey as a single-page app:
//    Register → Vault (Groq + Health Check) → Generate Workout → Verify Cards
//
//  NOTE: The app uses React state-based navigation (no URL routing).
//        All screens render at the same URL (localhost:5173)
// ─────────────────────────────────────────────────────────

const TEST_EMAIL = `e2e_pw_${Date.now()}@gymbrain.com`;
const TEST_PASSWORD = 'Pass123!';
// Load Groq API key from environment: set GROQ_API_KEY before running tests
const GROQ_API_KEY = process.env.GROQ_API_KEY || 'your-groq-api-key-here';

test.describe('🧠 GymBrain Full E2E Journey', () => {
    test.describe.configure({ mode: 'serial' });

    // ─────────────────────────────
    // 1️⃣  REGISTER
    // ─────────────────────────────
    test('1️⃣ Register a new user account', async ({ page }) => {
        await page.goto('/');

        // Should see the auth page with "Welcome Back" / "GymBrain"
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

        // Wait for health check + vaulting to complete (up to 30s for LLM round-trip)
        // On success, it shows a success message briefly then transitions to workout page
        await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 30_000 });

        // Screenshot: Workout page (health check passed!)
        await page.screenshot({ path: 'e2e/screenshots/04-ready-to-train.png' });

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

        // Since key is already vaulted, screen goes to 'vault' first, but we
        // need to check -- the app may go to vault or workout depending on state.
        // Let's wait for either Vault or Ready to Train
        await Promise.race([
            expect(page.getByText('Vault Setup')).toBeVisible({ timeout: 10_000 }),
            expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 10_000 }),
        ]);

        // If we're on vault, skip to workout
        if (await page.getByText('Vault Setup').isVisible()) {
            await page.getByText('Skip for now').click();
            await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 5_000 });
        }

        // Now we're on the workout page
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
        const exerciseCards = page.locator('.set-tracker');
        const exerciseCount = await exerciseCards.count();
        expect(exerciseCount).toBeGreaterThanOrEqual(3);

        // 5. Each exercise has name, sets, reps, weight
        for (let i = 0; i < exerciseCount; i++) {
            const card = exerciseCards.nth(i);
            const name = await card.locator('.exercise-name').textContent();
            const statValues = await card.locator('.stat-value').allTextContents();

            expect(name).toBeTruthy();
            expect(statValues.length).toBeGreaterThanOrEqual(3);

            console.log(`  💪 ${name} | ${statValues[0]} sets × ${statValues[1]} reps @ ${statValues[2]}kg`);
        }

        // 6. NO "No workout components" error
        await expect(page.getByText('No workout components')).not.toBeVisible();

        // 7. "Generate New Workout" button is visible
        await expect(page.getByText(/generate new workout/i)).toBeVisible();

        console.log(`\n✅ Workout generated with ${toneCount} tone card(s) and ${exerciseCount} exercise(s)!`);
    });

    // ─── Helper: Login → Skip Vault → Ready to Train ───
    async function loginToWorkoutPage(page: import('@playwright/test').Page) {
        await page.goto('/');
        await page.fill('#email', TEST_EMAIL);
        await page.fill('#password', TEST_PASSWORD);
        await page.getByRole('button', { name: /sign in/i }).click();

        // Wait for either Vault or Ready to Train to appear
        await page.waitForTimeout(2_000);

        // If we landed on Vault, skip to workout
        const skipBtn = page.getByText('Skip for now');
        if (await skipBtn.isVisible()) {
            await skipBtn.click();
        }

        await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 10_000 });
    }

    // ─────────────────────────────
    // 4️⃣  RESET & RE-GENERATE
    // ─────────────────────────────
    test('4️⃣ Reset workout and return to ready state', async ({ page }) => {
        await loginToWorkoutPage(page);

        // Generate a workout 
        await page.getByRole('button', { name: /generate workout/i }).click();
        await expect(page.getByText('Your Workout')).toBeVisible({ timeout: 60_000 });

        // Click "Generate New Workout"
        await page.getByText('← Generate New Workout').click();

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
        await loginToWorkoutPage(page);

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
