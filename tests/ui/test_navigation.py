import os
import time
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options

BASE_URL = os.getenv("BASE_URL", "http://localhost:5269")

def make_driver():
    opts = Options()
    # Uncomment next line if you want headless mode:
    # opts.add_argument("--headless=new")
    opts.add_argument("--window-size=1280,900")
    return webdriver.Chrome(options=opts)

def test_home_shop_links_navigate():
    driver = make_driver()
    try:
        driver.get(f"{BASE_URL}/")

        # Click Item Shop
        item_shop = driver.find_element(By.CSS_SELECTOR, ".hero-item .hero-btn")
        item_shop.click()
        time.sleep(0.5)

        assert "/Listings/Items" in driver.current_url, f"Expected Items URL, got {driver.current_url}"

        # Go back and click Lease Shop
        driver.back()
        time.sleep(0.5)

        lease_shop = driver.find_element(By.CSS_SELECTOR, ".hero-lease .hero-btn")
        lease_shop.click()
        time.sleep(0.5)

        assert "/Listings/Leases" in driver.current_url, f"Expected Leases URL, got {driver.current_url}"

        print("✅ Navigation test passed.")
    finally:
        driver.quit()

if __name__ == "__main__":
    test_home_shop_links_navigate()