using OnHive.Core.Library.Enums.Students;

namespace OnHive.Core.Library.Entities.Students
{
    public class StudentActivity : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public StudentEventTypes? EventType { get; set; }

        public DateTime? ActivityDate { get; set; }

        public string CourseId { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public string LessonId { get; set; } = string.Empty;

        public string LessonName { get; set; } = string.Empty;

        public string Event { get; set; } = string.Empty;

        public string EventDescription { get; set; } = string.Empty;
    }
}