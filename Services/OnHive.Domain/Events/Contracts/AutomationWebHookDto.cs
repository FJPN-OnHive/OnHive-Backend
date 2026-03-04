using OnHive.Core.Library.Enums.Events;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Events
{
    public class AutomationWebHookDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public AutomationWebHookMethod Method { get; set; } = AutomationWebHookMethod.GET;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = "application/json";

        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; } = [];
    }
}