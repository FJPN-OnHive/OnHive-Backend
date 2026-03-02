using EHive.Core.Library.Enums.Students;
using MongoDB.Bson.Serialization.Attributes;

namespace EHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class StudentCourse
    {
        public string Id { get; set; } = string.Empty;

        public string VId { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        public string OrderId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public StudentCourseState State { get; set; } = StudentCourseState.New;

        public string EnrollmentCode { get; set; } = string.Empty;

        public List<StudentDiscipline> Disciplines { get; set; } = new();

        public List<StudentAnnotation> Annotations { get; set; } = new();

        public string CertificateId { get; set; } = string.Empty;

        public DateTime LastAccessedDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}