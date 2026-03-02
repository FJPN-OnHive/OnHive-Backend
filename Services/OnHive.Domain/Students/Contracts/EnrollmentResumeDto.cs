using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class EnrollmentResumeDto
    {
        [JsonPropertyName("tenantId")]
        public string? TenantId { get; set; }

        [JsonPropertyName("courseId")]
        public string? CourseId { get; set; }

        [JsonPropertyName("courseName")]
        public string? CourseName { get; set; }

        [JsonPropertyName("courseCode")]
        public string? CourseCode { get; set; }

        [JsonPropertyName("lastEnrollmentDate")]
        public DateTime LastEnrollmentDate { get; set; }

        [JsonPropertyName("lastAccess")]
        public DateTime LastAccess { get; set; }

        [JsonPropertyName("totalStudents")]
        public int TotalStudents { get; set; } = 0;

        [JsonPropertyName("certificatesEmitted")]
        public int CertificatesEmitted { get; set; }
    }
}