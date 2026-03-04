using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class BankSlipInfoDto
    {
        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("assignor")]
        public string Assignor { get; set; } = string.Empty;

        [JsonPropertyName("demonstrative")]
        public string Demonstrative { get; set; } = string.Empty;

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = string.Empty;

        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;
    }
}