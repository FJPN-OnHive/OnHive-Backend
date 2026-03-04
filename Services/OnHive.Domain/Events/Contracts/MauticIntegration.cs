using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Events
{
    public class MauticIntegrationDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("integrationAPIKey")]
        public string IntegrationAPIKey { get; set; } = string.Empty;

        [JsonPropertyName("mauticUrl")]
        public string MauticUrl { get; set; } = string.Empty;

        [JsonPropertyName("mauticClientId")]
        public string MauticClientId { get; set; } = string.Empty;

        [JsonPropertyName("mauticClientSecret")]
        public string MauticClientSecret { get; set; } = string.Empty;

        [JsonPropertyName("mauticAccessToken")]
        public string MauticAccessToken { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}