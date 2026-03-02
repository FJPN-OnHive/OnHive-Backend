namespace EHive.Core.Library.Entities.Certificates
{
    public class CertificateMount : EntityBase
    {
        public string CertificateId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string StudentId { get; set; } = string.Empty;

        public string CourseId { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime EmissionDate { get; set; }

        public string CertificateKey { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public string ValidationUrl { get; set; } = string.Empty;
    }
}