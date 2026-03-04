using System;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentQuestionResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("responseOptions")]
        public List<string> ResponseOptions { get; set; } = [];

        [JsonPropertyName("responseText")]
        public string ResponseText { get; set; } = string.Empty;
    }
}