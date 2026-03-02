using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class EnrollmentDto
    {
        [JsonPropertyName("tenantId")]
        public string? TenantId { get; set; }

        [JsonPropertyName("studentId")]
        public string? StudentId { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("studentName")]
        public string? StudentName { get; set; }

        [JsonPropertyName("studentEmail")]
        public string? StudentEmail { get; set; }

        [JsonPropertyName("studentCpf")]
        public string? StudentCpf { get; set; }

        [JsonPropertyName("courseId")]
        public string? CourseId { get; set; }

        [JsonPropertyName("courseName")]
        public string? CourseName { get; set; }

        [JsonPropertyName("courseCode")]
        public string? CourseCode { get; set; }

        [JsonPropertyName("enrollmentDate")]
        public DateTime EnrollmentDate { get; set; }

        [JsonPropertyName("lastAccess")]
        public DateTime LastAccess { get; set; }

        [JsonPropertyName("courseProgress")]
        public int CourseProgress { get; set; } = 0;

        [JsonPropertyName("courseFinishDate")]
        public DateTime CourseFinishDate { get; set; }

        [JsonPropertyName("certificateEmitted")]
        public bool CertificateEmitted { get; set; } = false;
    }
}