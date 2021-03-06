﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebDriver.Docker.Selenium;

namespace WebDriver.Docker.Http
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

                    var results = new Dictionary<string, string[]>();
                    foreach (var test in side.Tests)
                    {
                        logger.LogInformation($"Starting test {test.Name}");
                        var chromeOptions = new ChromeOptions();
                        chromeOptions.AddArguments("--headless", "--no-sandbox", "--disable-gpu");
                        var service = ChromeDriverService.CreateDefaultService("/usr/bin/", "chromedriver");
                        var testResults = new List<string>(test.Commands.Length);
                        using (var driver = new Driver(new ChromeDriver(service, chromeOptions)))
                        {
                            driver.OnCommandExecuting += (sender, e) =>
                            {
                                logger.LogInformation($"Executing : {e.Command.Id} | {e.Command.Action} | {e.Command.Target}");
                            };

                            driver.OnCommandExecuted += (sender, e) =>
                            {
                                testResults.Add($"Executed : {e.Command.Id} | {e.Command.Action} | {e.Command.Target}");
                                logger.LogInformation($"Executed : {e.Command.Id} | {e.Command.Action} | {e.Command.Target}");
                            };

                            driver.Execute(test, side.Url);
                        }
                        results.Add(test.Id, testResults.ToArray());
                    }

                    return new OkObjectResult(results);
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
