# Automation of selenium test suite with Azure Functions

This Azure Function exposes one API accepting a JSON representation of a Selenium test case. The functions runs on a Docker container where a Chrome driver is installed. The ChromeDriver triggers an headless Chrome when the function is triggered and executes the test case.

The parser of the Selenium driver is still a work in progress and handles the following commands : 

- click
- open
- sendKeys
- setWindowsSize
- type
- waitForElementPresent

The following element selectors are supported :

- css
- id