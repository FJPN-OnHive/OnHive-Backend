using OnHive.Core.Library.Enums.Students;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentExamResultDto
    {
        [JsonPropertyName("score")]
        public double Score { get; set; } = 0;

        [JsonPropertyName("submitDate")]
        public DateTime SubmitDate { get; set; }

        [JsonPropertyName("state")]
        public StudentExamState State { get; set; } = StudentExamState.Pending;

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [JsonPropertyName("remainingRetries")]
        public int RemainingRetries { get; set; } = 0;

        [JsonPropertyName("try")]
        public int Try { get; set; } = 0;

        [JsonPropertyName("responses")]
        public List<StudentExamQuestionDto> Responses { get; set; } = [];
    }
}