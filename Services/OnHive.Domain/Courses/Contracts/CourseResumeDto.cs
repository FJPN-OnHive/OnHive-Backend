using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Courses
{
    public class CourseResumeDto
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

        [JsonPropertyName("code")]
        [Required]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("totalTimeMinutes")]
        public int TotalTimeMinutes { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("thumbnail")]
        [Required]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("imageUrl")]
        [Required]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("metaData")]
        public List<string> MetaData { get; set; } = new();

        [JsonPropertyName("relatedCourses")]
        public List<string> RelatedCourses { get; set; } = [];

        [JsonPropertyName("rate")]
        public int Rate { get; set; } = 0;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("difficultLevel")]
        public int DifficultLevel { get; set; } = 1;

        [JsonPropertyName("requirements")]
        public List<string> Requirements { get; set; } = [];

        [JsonPropertyName("duration")]
        public int Duration { get; set; } = 0;

        [JsonPropertyName("product")]
        public CourseProductDto? Product { get; set; }

        [JsonPropertyName("staff")]
        public List<CourseStaffDto> Staff { get; set; } = [];

        [JsonPropertyName("disciplines")]
        public List<DisciplineResumeDto> Disciplines { get; set; } = new();

        [JsonPropertyName("hasCertificate")]
        public bool HasCertificate { get; set; } = false;
    }
}