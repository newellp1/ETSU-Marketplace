import { test as setup } from '@playwright/test';

setup('register test user', async ({ page, baseURL }) => {
  await page.goto('/'); 
  await page.getByRole('link', { name: 'Log in' }).click();
  await page.getByRole('link', { name: 'Register' }).click();

  await page.getByRole('textbox', { name: 'Email address' }).fill('test@etsu.edu');
  await page.getByRole('textbox', { name: 'Password', exact: true }).fill('Testing1!');
  await page.getByRole('textbox', { name: 'Confirm Password' }).fill('Testing1!');
  await page.getByRole('textbox', { name: 'First Name' }).fill('Test');
  await page.getByRole('textbox', { name: 'Last Name' }).fill('User');

  await page.getByRole('button', { name: 'Continue' }).click();

  // --- "Username Already Taken" Logic ---
  
  // Check if we are still on the Register page instead of being redirected
  // Look for a validation summary or text that says "already taken" or "Duplicate"
  const errorText = page.getByText(/already taken|Duplicate|already exists/i);
  
  try {
    await errorText.waitFor({ state: 'visible', timeout: 3000 });
  } catch (e) { // success
    await page.waitForURL('**/'); 
  }
});