using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentAnnotationDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("position")]
        public int Position { get; set; } = 0;

        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}