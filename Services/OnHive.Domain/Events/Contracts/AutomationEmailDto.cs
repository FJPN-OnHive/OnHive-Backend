using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Events
{
    public class AutomationEmailDto
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;
    }
}