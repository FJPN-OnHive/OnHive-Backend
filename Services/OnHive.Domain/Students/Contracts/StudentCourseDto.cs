using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentCourseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("versionNumber")]
        public string VersionNumber { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public StudentCourseState State { get; set; } = StudentCourseState.New;

        [JsonPropertyName("enrollmentCode")]
        public string EnrollmentCode { get; set; } = string.Empty;

        [JsonPropertyName("disciplines")]
        public List<StudentDisciplineDto> Disciplines { get; set; } = new();

        [JsonPropertyName("annotations")]
        public List<StudentAnnotationDto> Annotations { get; set; } = new();

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("isLastAccessed")]
        public bool IsLastAccessed { get; set; } = false;

        [JsonPropertyName("lastAccessedDate")]
        public DateTime LastAccessedDate { get; set; }

        [JsonPropertyName("certificateId")]
        public string CertificateId { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
    }
}