using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace WebDriver.Selenium
{
    public class Driver : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public event CommandEvent CommandExecuting;
        public event CommandEvent CommandExecuted;

        public Driver(IWebDriver driver) : this(driver, TimeSpan.FromSeconds(15)) { }
        public Driver(IWebDriver driver, TimeSpan waitTimeout)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, waitTimeout);
        }

        public void Execute(Test test, string rootUrl)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            if (string.IsNullOrWhiteSpace(rootUrl))
            {
                throw new ArgumentNullException(nameof(rootUrl));
            }

            if (test.Commands == null
                || test.Commands.Length == 0
                || test.Commands.Any(c => string.IsNullOrWhiteSpace(c?.Action) || string.IsNullOrWhiteSpace(c.Target)))
            {
                throw new ArgumentException(nameof(test.Commands));
            }

            _driver.Navigate().GoToUrl(rootUrl);
            foreach (var command in test.Commands)
            {
                ExecuteCommand(command);
            }

        }

        private void ExecuteCommand(Command command)
        {
            OnCommandExecuting(command);
            switch (command.Action.ToLowerInvariant())
            {
                case "open":
                    if (!string.Equals(_driver.Url, command.Target))
                    {
                        _driver.Navigate().GoToUrl(command.Target);
                    }
                    break;

                case "setwindowsize":
                    var dimensions = command.Target.Split('x');
                    _driver.Manage().Window.Size = new System.Drawing.Size(int.Parse(dimensions[0]), int.Parse(dimensions[1]));
                    break;

                case "click":
                    GetElement(command.Target).Click();
                    break;

                case "type":
                case "sendkeys":
                    GetElement(command.Target).SendKeys(command.Value);
                    break;

                default:
                    throw new NotImplementedException($"{command.Action} not implemented");
            }

            OnCommandExecuted(command);
        }

        private IWebElement GetElement(string target)
        {
            var attributes = target.Split('=');
            var selector = attributes[0];
            var selectorValue = attributes[1];

            switch (selector)
            {
                case "id":
                    _wait.Until(driver => driver.FindElement(By.Id(selectorValue)));
                    return _driver.FindElement(By.Id(selectorValue));

                default:
                    throw new NotImplementedException($"Click on {selector} not implemented");
            }
        }

        private void OnCommandExecuting(Command command)
        {
            CommandExecuting?.Invoke(this, new CommandEventArgs(command));
        }

        private void OnCommandExecuted(Command command)
        {
            CommandExecuted?.Invoke(this, new CommandEventArgs(command));
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
        }
    }
}
