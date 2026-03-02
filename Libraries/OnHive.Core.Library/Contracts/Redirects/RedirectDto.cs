using EHive.Core.Library.Enums.Redirects;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Redirects
{
    public class RedirectDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("passParameters")]
        public bool PassParameters { get; set; } = true;

        [JsonPropertyName("type")]
        public RedirectType Type { get; set; } = RedirectType.Temporary;
    }
}