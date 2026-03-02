using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentExamQuestionOptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("letter")]
        public string Letter { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}