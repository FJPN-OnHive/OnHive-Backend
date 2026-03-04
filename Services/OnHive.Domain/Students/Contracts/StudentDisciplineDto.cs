using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentDisciplineDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("versionNumber")]
        public string VersionNumber { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 1;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("progress")]
        public int Progress => Lessons.Count > 0 ? (int)Math.Round((Lessons.Count(l => l.State == StudentLessonState.Finished) / (double)Lessons.Count) * 100) : 0;

        [JsonPropertyName("lessons")]
        public List<StudentLessonsDto> Lessons { get; set; } = new();
    }
}