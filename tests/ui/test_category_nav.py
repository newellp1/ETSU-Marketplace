import os
import time
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options

BASE_URL = os.getenv("BASE_URL", "http://localhost:5269")

def make_driver():
    opts = Options()
    # opts.add_argument("--headless=new")
    opts.add_argument("--window-size=1280,900")
    return webdriver.Chrome(options=opts)

def test_category_nav_gaming_routes_to_items():
    driver = make_driver()
    try:
        driver.get(f"{BASE_URL}/")
        # Click category link in the nav
        driver.find_element(By.LINK_TEXT, "Gaming").click()
        time.sleep(0.5)

        assert "/Listings/Items" in driver.current_url, driver.current_url
        assert "category=Gaming" in driver.current_url, driver.current_url

        print("✅ Category nav test passed.")
    finally:
        driver.quit()

if __name__ == "__main__":
    test_category_nav_gaming_routes_to_items()