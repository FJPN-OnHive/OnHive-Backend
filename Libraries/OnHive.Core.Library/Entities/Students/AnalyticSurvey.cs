using EHive.Core.Library.Entities.Courses;

namespace EHive.Core.Library.Entities.Students
{
    public class AnalyticSurvey
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string CPF { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public StudentExam? StudentExam { get; set; }

        public Exam? Exam { get; set; }
    }
}