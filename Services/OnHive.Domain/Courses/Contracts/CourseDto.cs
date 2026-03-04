using OnHive.Core.Library.Abstractions.Enrich;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class CourseDto : IEnrichable
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
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string? Body { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [JsonPropertyName("disciplines")]
        public List<DisciplineDto>? Disciplines { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("totalTimeMinutes")]
        public int? TotalTimeMinutes { get; set; }

        [JsonPropertyName("staff")]
        public List<CourseStaffDto>? Staff { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("metaData")]
        public List<string>? MetaData { get; set; }

        [JsonPropertyName("relatedCourses")]
        public List<string>? RelatedCourses { get; set; }

        [JsonPropertyName("rate")]
        public double? Rate { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("difficultLevel")]
        public int? DifficultLevel { get; set; }

        [JsonPropertyName("requirements")]
        public List<string>? Requirements { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("launchDate")]
        public DateTime? LaunchDate { get; set; }

        [JsonPropertyName("approvalAverage")]
        public double? ApprovalAverage { get; set; }

        [JsonPropertyName("lessonsCount")]
        public int LessonsCount { get; set; } = 0;

        [JsonPropertyName("materialsCount")]
        public int? MaterialsCount { get; set; }

        [JsonPropertyName("hasCertificate")]
        public bool HasCertificate { get; set; } = false;

        [JsonPropertyName("certificateId")]
        public string? CertificateId { get; set; }

        [JsonPropertyName("customAttributes")]
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }
}