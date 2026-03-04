using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Emails
{
    public class EmailSendDto
    {
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("templateCode")]
        public string TemplateCode { get; set; } = string.Empty;

        [JsonPropertyName("accountCode")]
        public string AccountCode { get; set; } = "DEFAULT";

        [JsonPropertyName("serviceCode")]
        public string ServiceCode { get; set; } = "DEFAULT";

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("sendTo")]
        public List<string> SendTo { get; set; } = new();

        [JsonPropertyName("attachments")]
        public List<string> Attachments { get; set; } = new();

        [JsonPropertyName("fields")]
        public Dictionary<string, string> Fields { get; set; } = new();
    }
}