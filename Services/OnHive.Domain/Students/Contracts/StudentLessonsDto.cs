using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Enums.Courses;
using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentLessonsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("disciplineId")]
        public string DisciplineId { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 1;

        [JsonPropertyName("type")]
        public LessonTypes Type { get; set; } = LessonTypes.Lesson;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public StudentLessonState State { get; set; } = StudentLessonState.Pending;

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("videoUrl")]
        public string VideoUrl { get; set; } = string.Empty;

        [JsonPropertyName("embeddedVideo")]
        public string EmbeddedVideo { get; set; } = string.Empty;

        [JsonPropertyName("articleUrl")]
        public string ArticleUrl { get; set; } = string.Empty;

        [JsonPropertyName("annotations")]
        public List<StudentAnnotationDto> Annotations { get; set; } = new();

        [JsonPropertyName("exam")]
        public StudentExamDto? Exam { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; } = 0;

        [JsonPropertyName("isLastAccessed")]
        public bool IsLastAccessed { get; set; } = false;

        [JsonPropertyName("lastAccessDate")]
        public DateTime LastAccessDate { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("totalTimeMinutes")]
        public int TotalTimeMinutes { get; set; }

        [JsonPropertyName("materials")]
        public List<MaterialDto>? Materials { get; set; }
    }
}