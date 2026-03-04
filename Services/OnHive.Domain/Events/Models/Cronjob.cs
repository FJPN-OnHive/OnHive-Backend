namespace OnHive.Events.Domain.Models
{
    public class Cronjob
    {
        public string Name { get; set; } = string.Empty;

        public string CronExpression { get; set; } = string.Empty;

        public string TriggerUrl { get; set; } = string.Empty;

        public string Method { get; set; } = "GET";

        public Dictionary<string, string> Headers { get; set; } = [];

        public string Body { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;
    }
}