using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace WebDriver.Docker.Selenium
{
    public class Driver : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly TimeSpan _defaultWaitTimeout;

        public event CommandEvent OnCommandExecuting;
        public event CommandEvent OnCommandExecuted;

        public Driver(IWebDriver driver) : this(driver, TimeSpan.FromSeconds(15)) { }
        public Driver(IWebDriver driver, TimeSpan waitTimeout)
        {
            _driver = driver;
            _defaultWaitTimeout = waitTimeout;
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
            OnCommandExecuting?.Invoke(this, new CommandEventArgs(command));
            switch (command.Action.ToLowerInvariant())
            {
                case "click":
                    GetElement(command.Target).Click();
                    break;

                case "open":
                    if (!string.Equals(_driver.Url, command.Target))
                    {
                        string target = null;
                        if (command.Target.StartsWith("http://") || command.Target.StartsWith("https://"))
                        {
                            target = command.Target;
                        }
                        else
                        {
                            var uri = new Uri(_driver.Url);
                            target = command.Target.StartsWith("/") ? $"{uri.Scheme}://{uri.Host}{target}" : $"{uri.Scheme}://{uri.Host}/{command.Target}";
                        }
                        _driver.Navigate().GoToUrl(target);
                    }
                    break;

                case "sendkeys":
                case "type":
                    var element = GetElement(command.Target);
                    if (string.IsNullOrEmpty(command.Value))
                    {
                        element.Clear();
                    }
                    else
                    {
                        element.SendKeys(command.Value);
                    }
                    break;

                case "setwindowsize":
                    var dimensions = command.Target.Split('x');
                    _driver.Manage().Window.Size = new System.Drawing.Size(int.Parse(dimensions[0]), int.Parse(dimensions[1]));
                    break;

                case "waitforelementpresent":
                    _ = GetElement(command.Target, string.IsNullOrWhiteSpace(command.Value) || !int.TryParse(command.Target, out var timeout) ? null : TimeSpan.FromMilliseconds(timeout) as TimeSpan?);
                    break;
            }

            OnCommandExecuted?.Invoke(this, new CommandEventArgs(command));
        }

        private IWebElement GetElement(string target, TimeSpan? waitTimeout = null)
        {
            var attributes = target.Split('=');
            var selector = attributes[0];
            var selectorValue = attributes[1];

            switch (selector)
            {
                case "id":
                    new WebDriverWait(_driver, waitTimeout ?? _defaultWaitTimeout).Until(driver => driver.FindElement(By.Id(selectorValue)));
                    return _driver.FindElement(By.Id(selectorValue));

                case "css":
                    new WebDriverWait(_driver, waitTimeout ?? _defaultWaitTimeout).Until(driver => driver.FindElement(By.CssSelector(selectorValue)));
                    return _driver.FindElement(By.CssSelector(selectorValue));

                default:
                    throw new NotImplementedException($"Click on {selector} not implemented");
            }
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
        }
    }
}
