using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentActivityDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("eventType")]
        public StudentEventTypes? EventType { get; set; }

        [JsonPropertyName("activityDate")]
        public DateTime? ActivityDate { get; set; }

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("lessonId")]
        public string LessonId { get; set; } = string.Empty;

        [JsonPropertyName("lessonName")]
        public string LessonName { get; set; } = string.Empty;

        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("eventDescription")]
        public string EventDescription { get; set; } = string.Empty;
    }
}