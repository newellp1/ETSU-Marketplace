import { test, expect } from '@playwright/test';

test('createItemListing', async ({ page }) => {
  await page.goto('http://localhost:5269/');
  await page.getByRole('link', { name: 'Log in' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).fill('test@etsu.edu');
  await page.getByRole('textbox', { name: 'Email address' }).press('Tab');
  await page.getByRole('textbox', { name: 'Password' }).fill('Testing1!');
  await page.getByRole('button', { name: 'Log in' }).click();
  await page.goto('http://localhost:5269/listings/items/create');
  await page.getByRole('checkbox').first().check();
  await page.getByLabel('Condition').selectOption('1');
  await page.getByRole('textbox', { name: 'Title' }).click();
  await page.getByRole('textbox', { name: 'Title' }).fill('Test title');
  await page.getByRole('textbox', { name: 'Title' }).press('Tab');
  await page.getByRole('textbox', { name: 'Description' }).fill('test desc');
  await page.getByRole('spinbutton', { name: 'Price' }).click();
  await page.getByRole('spinbutton', { name: 'Price' }).fill('20');
  await page.getByRole('button', { name: 'Choose File' }).click();
  await page.getByRole('button', { name: 'Choose File' }).setInputFiles('./e2e/default-avatar.jpg');
  await page.getByRole('button', { name: 'Create' }).click();
});

test('createLeaseListing', async ({ page }) => {
  await page.goto('http://localhost:5269/');
  await page.getByRole('link', { name: 'Log in' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).fill('test@etsu.edu');
  await page.getByRole('textbox', { name: 'Email address' }).press('Tab');
  await page.getByRole('textbox', { name: 'Password' }).fill('Testing1!');
  await page.getByRole('button', { name: 'Log in' }).click();
  await page.goto('http://localhost:5269/listings/leases/create');
  await page.getByRole('textbox', { name: 'Address' }).click();
  await page.getByRole('textbox', { name: 'Address' }).fill('test');
  await page.getByRole('textbox', { name: 'Address' }).press('Tab');
  await page.getByRole('textbox', { name: 'LeaseStart' }).fill('2026-01-01');
  await page.getByRole('textbox', { name: 'LeaseEnd' }).fill('2027-01-01');
  await page.getByRole('checkbox', { name: 'UtilitiesIncluded' }).check();
  await page.getByRole('textbox', { name: 'Title' }).click();
  await page.getByRole('textbox', { name: 'Title' }).fill('test lease');
  await page.getByRole('textbox', { name: 'Title' }).press('Tab');
  await page.getByRole('textbox', { name: 'Description' }).fill('test descr');
  await page.getByRole('spinbutton', { name: 'Price' }).click();
  await page.getByRole('spinbutton', { name: 'Price' }).fill('1000');
  await page.getByRole('button', { name: 'Choose File' }).click();
  await page.getByRole('button', { name: 'Choose File' }).setInputFiles('./e2e/default-avatar.jpg');
  await page.getByRole('button', { name: 'Create' }).click();
});