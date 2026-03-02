using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Payments
{
    public class ProviderInfoDto
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("mfeUrl")]
        public string MfeUrl { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isAsync")]
        public bool IsAsync { get; set; }

        [JsonPropertyName("fields")]
        public List<ProviderField> Fields { get; set; } = new();

        [JsonPropertyName("paymentTypes")]
        public List<PaymentTypeDto> PaymentTypes { get; set; } = new();
    }

    public class ProviderField
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }
}