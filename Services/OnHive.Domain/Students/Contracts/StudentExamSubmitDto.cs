using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentExamSubmitDto
    {
        [JsonPropertyName("lessonId")]
        public string LessonId { get; set; } = string.Empty;

        [JsonPropertyName("examId")]
        public string ExamId { get; set; } = string.Empty;

        [JsonPropertyName("questions")]
        public List<StudentQuestionResponseDto> Questions { get; set; } = new();
    }
}