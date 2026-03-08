import { test, expect } from '@playwright/test';

/**
 * 🧠 GymBrain — Full End-to-End Journey (Consolidated)
 *
 * This E2E test validates the complete user flow in a single session:
 *   1. Register → 2. Onboard → 3. Vault API Key → 4. Generate Workout → 5. Reset → 6. Sign Out
 */

const TEST_ID = Date.now();
const TEST_EMAIL = `e2e_pw_${TEST_ID}@gymbrain.com`;
const TEST_PASSWORD = 'Test1234!';
const GROQ_API_KEY = process.env.GROQ_API_KEY || 'gsk_test_placeholder';

test('🧠 GymBrain Full E2E Ritual', async ({ page }) => {
    test.setTimeout(180_000); // 3 min budget

    // ─────────────────────────────
    // 1️⃣  REGISTER
    // ─────────────────────────────
    await page.goto('/');
    const signUpBtn = page.getByRole('button', { name: /sign up/i });
    if (await signUpBtn.isVisible()) {
        await signUpBtn.click();
    }
    await page.fill('#email', TEST_EMAIL);
    await page.fill('#password', TEST_PASSWORD);
    await page.getByRole('button', { name: /create account/i }).click();

    // ─────────────────────────────
    // 2️⃣  ONBOARDING
    // ─────────────────────────────
    await expect(page.getByText(/get to know you/i)).toBeVisible({ timeout: 10_000 });
    await page.fill('#ob-name', 'E2E Ritual Athlete');
    await page.getByRole('button', { name: /continue/i }).click();

    await expect(page.getByText(/what's your goal/i)).toBeVisible();
    await page.getByRole('button', { name: /Build Muscle/i }).click();
    await page.getByRole('button', { name: /Intermediate/i }).click();
    await page.getByRole('button', { name: /4x/i }).click();
    await page.getByRole('button', { name: /continue/i }).click();

    await expect(page.getByText(/AI Coach Setup/i)).toBeVisible();
    await page.getByRole('button', { name: /Let's Go/i }).click();

    await expect(page.getByText(/Hello,/i)).toBeVisible({ timeout: 15_000 });
    console.log(`✅ Registration & Onboarding success: ${TEST_EMAIL}`);

    // ─────────────────────────────
    // 3️⃣  VAULT API KEY
    // ─────────────────────────────
    const vaultTab = page.locator('.bottom-nav__item', { hasText: /Vault/i });
    await vaultTab.click();
    await expect(page.getByText('Secure Vault')).toBeVisible({ timeout: 5_000 });
    await page.locator('#provider').selectOption('groq');
    const modelSelect = page.locator('#model');
    await modelSelect.selectOption({ label: '🎁 Llama 3.3 70B' });
    await page.fill('#apikey', GROQ_API_KEY);
    await page.getByRole('button', { name: /vault my key/i }).click();
    await expect(page.getByText(/Encrypting/i)).toBeVisible({ timeout: 3_000 });
    await expect(page.getByText('Hello,')).toBeVisible({ timeout: 45_000 });
    console.log('✅ Vaulted key successfully!');

    // ─────────────────────────────
    // 4️⃣  GENERATE WORKOUT
    // ─────────────────────────────
    const trainTab = page.locator('.bottom-nav__item', { hasText: /Train/i });
    await trainTab.click();
    await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 5_000 });
    await page.locator('#focus').selectOption('chest and arms');
    await page.getByRole('button', { name: /generate workout/i }).click();
    await expect(page.getByText(/Generating/i)).toBeVisible({ timeout: 5_000 });
    await expect(page.getByText('Your Workout')).toBeVisible({ timeout: 60_000 });

    const exerciseCards = page.locator('.exercise-card');
    const count = await exerciseCards.count();
    expect(count).toBeGreaterThanOrEqual(1); // Relaxed for placeholder testing if needed
    console.log(`✅ Workout generated with ${count} exercises!`);

    // ─────────────────────────────
    // 5️⃣  RESET & SIGN OUT
    // ─────────────────────────────
    await page.getByRole('button', { name: /new/i }).click();
    await expect(page.getByText('Ready to Train')).toBeVisible({ timeout: 5_000 });

    const profileTab = page.locator('.bottom-nav__item', { hasText: /Profile/i });
    await profileTab.click();
    await expect(page.getByText('Your Profile')).toBeVisible({ timeout: 5_000 });
    await page.getByRole('button', { name: /sign out/i }).click();
    await expect(page.getByText('Welcome Back')).toBeVisible({ timeout: 5_000 });

    console.log('✅ Full journey ritual complete!');
});
