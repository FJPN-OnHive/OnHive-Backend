using OnHive.Core.Library.Enums.Courses;
using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentExamDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("lessonId")]
        public string LessonId { get; set; } = string.Empty;

        [JsonPropertyName("examId")]
        public string ExamId { get; set; } = string.Empty;

        [JsonPropertyName("examVersionNumber")]
        public int ExamVersionNumber { get; set; } = 1;

        [JsonPropertyName("state")]
        public StudentExamState State { get; set; } = StudentExamState.Pending;

        [JsonPropertyName("type")]
        public ExamTypes Type { get; set; } = ExamTypes.Exam;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("requiredLessons")]
        public List<string> RequiredLessons { get; set; } = new();

        [JsonPropertyName("timeProgress")]
        public int TimeProgress { get; set; } = 0;

        [JsonPropertyName("submitDate")]
        public DateTime SubmitDate { get; set; }

        [JsonPropertyName("totalScore")]
        public double TotalScore { get; set; } = 0.0;

        [JsonPropertyName("studentScore")]
        public double StudentScore { get; set; } = 0.0;

        [JsonPropertyName("requiredScore")]
        public double RequiredScore { get; set; } = 0.0;

        [JsonPropertyName("questions")]
        public List<StudentExamQuestionDto> Questions { get; set; } = new();

        [JsonPropertyName("maxRetries")]
        public int MaxRetries { get; set; } = 1;

        [JsonPropertyName("currentTry")]
        public int CurrentTry { get; set; } = 0;
    }
}