# ETSU Marketplace - UI Tests (Selenium)

## Prereqs
- Python installed
- Selenium installed:
  - `pip install selenium`
- Google Chrome installed

## Run the web app (Terminal 1)
From the project folder:
`dotnet watch run`

Confirm it prints a URL like:
`Now listening on: http://localhost:5269`

## Run UI tests (Terminal 2)
From the project folder:
- `python tests/ui/test_navigation.py`
- `python tests/ui/test_category_nav.py`

## Optional: set a different base URL
If your app uses a different port:
- PowerShell:
  - `$env:BASE_URL="http://localhost:####"`
  - `python tests/ui/test_navigation.py`