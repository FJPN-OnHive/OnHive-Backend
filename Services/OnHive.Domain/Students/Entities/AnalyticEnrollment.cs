using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Entities.Students
{
    public class AnalyticEnrollment
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string CPF { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public DateTime LastAccessedDate { get; set; }

        public string State { get; set; } = string.Empty;

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public DateTime EndTime { get; set; }

        public DateTime CertificateDate { get; set; }

        public StudentCourse? Course { get; set; }
    }
}