using EHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentCourseResumeDto
    {
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("versionNumber")]
        public string VersionNumber { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public StudentCourseState State { get; set; } = StudentCourseState.New;

        [JsonPropertyName("disciplines")]
        public List<StudentDisciplineResumeDto> Disciplines { get; set; } = new();

        [JsonPropertyName("progress")]
        public int Progress { get; set; } = 0;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("isLastAccessed")]
        public bool IsLastAccessed { get; set; } = false;

        [JsonPropertyName("lastAccessedDate")]
        public DateTime LastAccessedDate { get; set; }

        [JsonPropertyName("hasCertificate")]
        public bool HasCertificate { get; set; } = true;

        [JsonPropertyName("certificateId")]
        public string CertificateId { get; set; } = string.Empty;
    }
}