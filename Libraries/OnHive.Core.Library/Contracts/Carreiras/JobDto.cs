using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Carreiras
{
    public class JobDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        [MaxLength(256)]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        [MaxLength(256)]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("initialDate")]
        public DateTime? InitialDate { get; set; }

        [JsonPropertyName("finalDate")]
        public DateTime? FinalDate { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 0;

        [JsonPropertyName("externalLink")]
        public string? ExternalLink { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}