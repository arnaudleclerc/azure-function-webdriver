namespace WebDriver.Docker.Selenium
{
    public class Side
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Test[] Tests { get; set; }
    }
}
