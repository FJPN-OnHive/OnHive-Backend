using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Tenants
{
    public class TenantResumeDto
    {
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}