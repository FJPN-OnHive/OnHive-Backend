using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Events
{
    public class IntegrationData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("conversion")]
        public string Conversion { get; set; } = string.Empty;

        [JsonPropertyName("formId")]
        public string FormId { get; set; } = string.Empty;

        [JsonPropertyName("formName")]
        public string FormName { get; set; } = string.Empty;

        [JsonPropertyName("acceptance")]
        public bool Acceptance { get; set; } = false;
    }
}