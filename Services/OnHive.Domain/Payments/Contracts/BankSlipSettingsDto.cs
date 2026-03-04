using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class BankSlipSettingsDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("assignor")]
        public string Assignor { get; set; } = string.Empty;

        [JsonPropertyName("demonstrative")]
        public string Demonstrative { get; set; } = string.Empty;

        [JsonPropertyName("expirationsDays")]
        public int ExpirationsDays { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = string.Empty;

        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;
    }
}