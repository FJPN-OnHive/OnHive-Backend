using EHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Courses
{
    public class ExamQuestionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("auxText")]
        public string AuxText { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public QuestionTypes Type { get; set; } = QuestionTypes.SingleChoice;

        [JsonPropertyName("options")]
        public List<QuestionOptionDto> Options { get; set; } = new();

        [JsonPropertyName("value")]
        public double Value { get; set; } = 0.0;

        [JsonPropertyName("optional")]
        public bool Optional { get; set; } = false;
    }

    public class QuestionOptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("letter")]
        public string Letter { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("isCorrect")]
        public bool IsCorrect { get; set; } = false;
    }
}