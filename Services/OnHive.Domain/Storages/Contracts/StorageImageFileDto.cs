using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Storages
{
    public class StorageImageFileDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("imageId")]
        [Required]
        public string ImageId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("altText")]
        [MaxLength(256)]
        public string AltText { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        [MaxLength(256)]
        public string Subtitle { get; set; } = string.Empty;

        [JsonPropertyName("originalFileName")]
        public string OriginalFileName { get; set; } = string.Empty;

        [JsonPropertyName("hiResImageUrl")]
        public string HiResImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("midResImageUrl")]
        public string MidResImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("lowResImageUrl")]
        public string LowResImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("public")]
        public bool Public { get; set; } = true;

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];
    }
}