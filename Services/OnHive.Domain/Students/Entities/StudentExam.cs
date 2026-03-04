using OnHive.Core.Library.Enums.Courses;
using OnHive.Core.Library.Enums.Students;
using MongoDB.Bson.Serialization.Attributes;

namespace OnHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class StudentExam : EntityBase
    {
        public string StudentId { get; set; } = string.Empty;

        public string CourseId { get; set; } = string.Empty;

        public string LessonId { get; set; } = string.Empty;

        public string ExamId { get; set; } = string.Empty;

        public string ExamVId { get; set; } = string.Empty;

        public int ExamVersionNumber { get; set; } = 1;

        public StudentExamState State { get; set; } = StudentExamState.Pending;

        public int TimeProgress { get; set; } = 0;

        public DateTime SubmitDate { get; set; }

        public double StudentScore { get; set; } = 0.0;

        public bool Approved { get; set; } = false;

        public List<StudentExamQuestion> Questions { get; set; } = new();

        public int CurrentTry { get; set; } = 0;
    }
}