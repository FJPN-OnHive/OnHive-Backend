using EHive.Core.Library.Contracts.Catalog;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Search
{
    public class ProductCourseSearchDto
    {
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imageAltText")]
        public string? ImageAltText { get; set; }

        [JsonPropertyName("itemUrl")]
        public string? ItemUrl { get; set; }

        [JsonPropertyName("totalTimeMinutes")]
        public int TotalTimeMinutes { get; set; } = 0;

        [JsonPropertyName("rate")]
        public float Rate { get; set; } = 0;

        [JsonPropertyName("difficultLevel")]
        public int DifficultLevel { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("fullPrice")]
        public float FullPrice { get; set; } = 0;

        [JsonPropertyName("lowPrice")]
        public float LowPrice { get; set; } = 0;

        [JsonPropertyName("sales")]
        public int Sales { get; set; } = 0;

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        [JsonPropertyName("launchDate")]
        public DateTime? LaunchDate { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("snapshotDate")]
        public DateTime SnapshotDate { get; set; }

        [JsonPropertyName("prices")]
        public List<ProductPriceDto>? Prices { get; set; } = new();

        [JsonPropertyName("externalUrl")]
        public string? ExternalUrl { get; set; }
    }
}