using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Certificates
{
    public class CertificateParameterDto
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}