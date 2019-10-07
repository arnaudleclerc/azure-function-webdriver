using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using WebDriver.Functions.Selenium;

namespace WebDriver.Functions.Http
{
    public class RunSide
    {
        [FunctionName("RunSide-HTTP")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "side")] HttpRequest req,
            ILogger logger)
        {
            try
            {
                using (var reader = new StreamReader(req.Body))
                {
                    var side = JsonConvert.DeserializeObject<Side>(reader.ReadToEnd());

                    if (side?.Tests == null
                        || side.Tests.Length == 0
                        || string.IsNullOrWhiteSpace(side.Url)
                        || side.Tests.Any(
                            test => test?.Commands == null
                            || test.Commands.Length == 0
                            || test.Commands.Any(c => string.IsNullOrWhiteSpace(c?.Action) || string.IsNullOrWhiteSpace(c.Target)))
                        )
                    {
                        return new BadRequestResult();
                    }

                    foreach (var test in side.Tests)
                    {
                        logger.LogInformation($"Starting test {test.Name}");
                        var chromeOptions = new ChromeOptions
                        {
                            BinaryLocation = Path.Combine(Environment.CurrentDirectory, "Chromium-77.0.3865.75-x64", "chrome.exe")
                        };
                        chromeOptions.AddArguments("--headless");
                        using (var driver = new Driver(new ChromeDriver(chromeOptions)))
                        {
                            driver.CommandExecuting += (sender, e) =>
                            {
                                logger.LogInformation($"Executing : {e.Command.Id} | {e.Command.Action} | {e.Command.Target}");
                            };

                            driver.CommandExecuted += (sender, e) =>
                            {
                                logger.LogInformation($"Executed : {e.Command.Id} | {e.Command.Action} | {e.Command.Target}");
                            };

                            driver.Execute(test, side.Url);
                        }
                    }

                    return new NoContentResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
