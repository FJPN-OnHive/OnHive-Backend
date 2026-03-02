using EHive.Core.Library.Enums.Videos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Videos
{
    public class VideoDto
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

        [JsonPropertyName("status")]
        public VideoStatus Status { get; set; } = VideoStatus.Processing;

        [JsonPropertyName("videoSource")]
        public string VideoSource { get; set; } = string.Empty;

        [JsonPropertyName("fileName")]
        [MaxLength(256)]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("sourceId")]
        [MaxLength(256)]
        public string SourceId { get; set; } = string.Empty;

        [JsonPropertyName("videoId")]
        [MaxLength(256)]
        public string VideoId { get; set; } = string.Empty;

        [JsonPropertyName("sourceFileUrl")]
        [MaxLength(512)]
        public string SourceFileUrl { get; set; } = string.Empty;

        [JsonPropertyName("videoUrl")]
        [MaxLength(512)]
        public string VideoUrl { get; set; } = string.Empty;

        [JsonPropertyName("embeddedVideo")]
        public string EmbeddedVideo { get; set; } = string.Empty;

        [JsonPropertyName("thumbnailUrl")]
        [MaxLength(512)]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];
    }
}