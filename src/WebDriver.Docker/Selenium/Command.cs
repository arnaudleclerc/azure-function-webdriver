using Newtonsoft.Json;

namespace WebDriver.Docker.Selenium
{
    public class Command
    {
        [JsonProperty("command")]
        public string Action { get; set; }
        public string Id { get; set; }
        public string Comment { get; set; }
        public string Target { get; set; }
        public string Value { get; set; }
    }
}
