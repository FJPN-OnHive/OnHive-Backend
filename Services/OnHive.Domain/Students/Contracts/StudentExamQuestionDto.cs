using OnHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentExamQuestionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("type")]
        public QuestionTypes Type { get; set; } = QuestionTypes.SingleChoice;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("auxText")]
        public string AuxText { get; set; } = string.Empty;

        [JsonPropertyName("questionsOptions")]
        public List<StudentExamQuestionOptionDto> QuestionsOptions { get; set; } = new();

        [JsonPropertyName("score")]
        public double Score { get; set; } = 0.0;

        [JsonPropertyName("questionScore")]
        public double QuestionScore { get; set; } = 0.0;

        [JsonPropertyName("correct")]
        public bool? Correct { get; set; } = false;

        [JsonPropertyName("responseText")]
        public string ResponseText { get; set; } = string.Empty;

        [JsonPropertyName("optional")]
        public bool Optional { get; set; } = false;

        [JsonPropertyName("responseOptions")]
        public List<string> ResponseOptions { get; set; } = [];
    }
}