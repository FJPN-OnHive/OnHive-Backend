using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Emails
{
    public class ComposedEmailDto
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("attachments")]
        public List<string> Attachments { get; set; } = new();

        [JsonPropertyName("sendTo")]
        public List<string> SendTo { get; set; } = new();

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("account")]
        public string Account { get; set; } = string.Empty;

        [JsonPropertyName("sendDate")]
        public DateTime SendDate { get; set; }
    }
}