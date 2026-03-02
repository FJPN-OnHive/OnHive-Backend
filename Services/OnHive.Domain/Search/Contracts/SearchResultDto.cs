using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Search
{
    public class SearchResultDto
    {
        [JsonPropertyName("type")]
        [MaxLength(128)]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("sourceId")]
        [MaxLength(256)]
        public string SourceId { get; set; } = string.Empty;

        [JsonPropertyName("sourceImageUrl")]
        public string? SourceImageUrl { get; set; }

        [JsonPropertyName("sourceImageAltText")]
        public string? SourceImageAltText { get; set; }

        [JsonPropertyName("sourceSlug")]
        public string? SourceSlug { get; set; }

        [JsonPropertyName("sourceUrl")]
        public string? SourceUrl { get; set; }

        [JsonPropertyName("sortDate")]
        public DateTime SortDate { get; set; }

        [JsonPropertyName("externalUrl")]
        public string? ExternalUrl { get; set; }
    }
}