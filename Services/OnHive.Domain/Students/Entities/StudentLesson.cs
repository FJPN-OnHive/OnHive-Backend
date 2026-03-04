using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Enums.Courses;
using OnHive.Core.Library.Enums.Students;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class StudentLesson
    {
        public string Id { get; set; } = string.Empty;

        public string VId { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        public string CourseId { get; set; } = string.Empty;

        public string DisciplineId { get; set; } = string.Empty;

        public LessonTypes Type { get; set; } = LessonTypes.Lesson;

        public StudentLessonState State { get; set; } = StudentLessonState.Pending;

        public List<StudentAnnotation> Annotations { get; set; } = new();

        public StudentExam? Exam { get; set; }

        public int Progress { get; set; } = 0;

        public DateTime LastAccessDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}