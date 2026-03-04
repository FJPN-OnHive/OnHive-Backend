using OnHive.Core.Library.Abstractions.Enrich;
using OnHive.Core.Library.Enums.Courses;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class LessonDto : IEnrichable
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1";

        [JsonPropertyName("versionNumber")]
        public int VersionNumber { get; set; } = 1;

        [JsonPropertyName("activeVersion")]
        public bool ActiveVersion { get; set; } = true;

        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public LessonTypes? Type { get; set; }

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("body")]
        [Required]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        [Required]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("order")]
        [Required]
        public int Order { get; set; } = 0;

        [JsonPropertyName("thumbnail")]
        [Required]
        public string Thumbnail { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("videoUrl")]
        public string? VideoUrl { get; set; }

        [JsonPropertyName("videoId")]
        public string? VideoId { get; set; }

        [JsonPropertyName("embeddedVideo")]
        public string? EmbeddedVideo { get; set; }

        [JsonPropertyName("articleUrl")]
        public string? ArticleUrl { get; set; }

        [JsonPropertyName("exam")]
        public ExamDto? Exam { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("staff")]
        public List<CourseStaffDto>? Staff { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("materials")]
        public List<MaterialDto>? Materials { get; set; }

        [JsonPropertyName("metaData")]
        public List<string>? MetaData { get; set; }

        [JsonPropertyName("totalTimeMinutes")]
        public int? TotalTimeMinutes { get; set; }

        [JsonPropertyName("customAttributes")]
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }
}