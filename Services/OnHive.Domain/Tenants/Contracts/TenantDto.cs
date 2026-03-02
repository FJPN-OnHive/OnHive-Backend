using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Tenants
{
    public class TenantDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("cnpj")]
        public string? CNPJ { get; set; }

        [JsonPropertyName("stateInscription")]
        public string? StateInscription { get; set; }

        [JsonPropertyName("cityInscription")]
        public string? CityInscription { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("features")]
        public List<string>? Features { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }
    }
}