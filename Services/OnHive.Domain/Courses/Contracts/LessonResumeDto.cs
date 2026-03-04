using OnHive.Core.Library.Enums.Courses;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class LessonResumeDto
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

        [JsonPropertyName("order")]
        public int Order { get; set; } = 1;

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public LessonTypes? Type { get; set; }

        public List<CourseStaffDto> Staff { get; set; } = new();
    }
}