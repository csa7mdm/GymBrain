import { test, expect } from '@playwright/test';

/**
 * 🎬 GymBrain — Automated Product Demo
 * 
 * This script conducts a high-quality walkthrough of the production features:
 * 1. Login with Admin credentials
 * 2. Home Page overview
 * 3. AI Vaulting (Premium Feature)
 * 4. AI Workout Generation
 * 5. Training Experience (Rest Timer + Sets)
 * 6. Profile Management
 */

const ADMIN_EMAIL = 'cs.a7md.m@gmail.com';
const ADMIN_PASSWORD = 'P@$sW0Rdz9090';
// Using a placeholder for the demo, or user can provide one. 
// For the demo recording, we just want to show the UI interaction.
const DEMO_API_KEY = 'gsk_demo_placeholder_12345';

test('🎬 GymBrain Production Showcase', async ({ page }) => {
    test.setTimeout(180_000);

    // Capture browser console logs
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));
    page.on('pageerror', err => console.log(`[BROWSER ERROR] ${err.message}`));

    // 1️⃣ LANDING & LOGIN
    await page.goto('/');
    await expect(page).toHaveTitle(/GymBrain/);

    // Check if we need to sign in
    const signInLink = page.getByRole('link', { name: /sign in/i });
    if (await signInLink.isVisible()) {
        await signInLink.click();
    }

    await page.fill('#email', ADMIN_EMAIL);
    await page.fill('#password', ADMIN_PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();

    // 2️⃣ HOME PAGE
    await expect(page.getByText(/Hello,/i)).toBeVisible({ timeout: 15_000 });
    await page.waitForTimeout(2000); // Wait for animations

    // 3️⃣ VAULT (Premium Feature Showcase)
    const vaultTab = page.locator('.bottom-nav__item', { hasText: /Vault/i });
    await vaultTab.click();
    await expect(page.getByText(/Secure Vault/i)).toBeVisible();

    // Select Groq and enter key (simulating premium setup)
    await page.locator('#provider').selectOption('groq');
    await page.fill('#apikey', DEMO_API_KEY);
    await page.getByRole('button', { name: /vault my key/i }).click();

    // Wait for the encryption success (return to home)
    await expect(page.getByText(/Hello,/i)).toBeVisible({ timeout: 10_000 });

    // 4️⃣ TRAIN (Workout Generation)
    const trainTab = page.locator('.bottom-nav__item', { hasText: /Train/i });
    await trainTab.click();
    await expect(page.getByText(/Ready to Train/i)).toBeVisible();

    // Configure a demo workout
    await page.locator('#focus').selectOption('full body');
    await page.getByRole('button', { name: /generate workout/i }).click();

    // Wait for the "AI is thinking" state then the workout
    await expect(page.getByText(/Generating/i)).toBeVisible();
    // In demo mode with a placeholder key, it might show an error or a mock.
    // We'll wait a bit to show the UI state.
    await page.waitForTimeout(5000);

    // 5️⃣ PROFILE & THEME
    const profileTab = page.locator('.bottom-nav__item', { hasText: /Profile/i });
    await profileTab.click();
    await expect(page.getByText(/Your Profile/i)).toBeVisible();

    // 6️⃣ LOGOUT
    await page.getByRole('button', { name: /sign out/i }).click();
    await expect(page.getByText(/Welcome Back/i)).toBeVisible();
});
