using EHive.Core.Library.Enums.Courses;
using EHive.Core.Library.Enums.Students;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentLessonsResumeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public LessonTypes Type { get; set; } = LessonTypes.Lesson;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 1;

        [JsonPropertyName("state")]
        public StudentLessonState State { get; set; } = StudentLessonState.Pending;

        [JsonPropertyName("progress")]
        public int Progress { get; set; } = 0;

        [JsonPropertyName("isLastAccessed")]
        public bool IsLastAccessed { get; set; } = false;

        [JsonPropertyName("lastAccessDate")]
        public DateTime LastAccessDate { get; set; }
    }
}