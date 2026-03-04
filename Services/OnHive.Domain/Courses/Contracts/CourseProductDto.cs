using OnHive.Core.Library.Enums.Payments;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class CourseProductDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        [MaxLength(100)]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imageAltText")]
        public string? ImageAltText { get; set; }

        [JsonPropertyName("externalId")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("fullPrice")]
        public double FullPrice { get; set; } = 0;

        [JsonPropertyName("lowPrice")]
        public double LowPrice { get; set; } = 0;

        [JsonPropertyName("sells")]
        public int Sells { get; set; } = 0;
    }
}