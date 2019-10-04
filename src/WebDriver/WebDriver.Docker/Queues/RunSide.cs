using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using WebDriver.Functions.Selenium;

namespace WebDriver.Functions.Queues
{
    public class RunSide
    {
        [FunctionName("RunSide-Queue")]
        public void Run([QueueTrigger("webdriver", Connection = "WebDriverStorageConnectionString")] string sideItem,
            ILogger logger)
        {
            try
            {
                var side = JsonConvert.DeserializeObject<Side>(sideItem);

                foreach (var test in side.Tests)
                {
                    logger.LogInformation($"Starting test {test.Name}");
                    using (var driver = new Driver(new ChromeDriver()))
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
