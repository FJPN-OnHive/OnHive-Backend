using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentLessonProgressDto
    {
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("lessonId")]
        public string LessonId { get; set; } = string.Empty;

        [JsonPropertyName("progress")]
        public int Progress { get; set; } = 0;

        [JsonPropertyName("state")]
        public StudentLessonState State { get; set; } = StudentLessonState.Pending;

        [JsonPropertyName("examSubmit")]
        public StudentExamSubmitDto? ExamSubmit { get; set; }
    }
}