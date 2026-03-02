using EHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentProgressResponseDto
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("lessonId")]
        public string LessonId { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("disciplineId")]
        public string DisciplineId { get; set; } = string.Empty;

        [JsonPropertyName("courseProgress")]
        public int CourseProgress { get; set; } = 0;

        [JsonPropertyName("courseState")]
        public StudentCourseState CourseState { get; set; } = StudentCourseState.New;

        [JsonPropertyName("lessonProgress")]
        public int LessonProgress { get; set; } = 0;

        [JsonPropertyName("lessonState")]
        public StudentLessonState LessonState { get; set; } = StudentLessonState.Pending;

        [JsonPropertyName("examResult")]
        public StudentExamResultDto? StudentExamResult { get; set; }
    }
}