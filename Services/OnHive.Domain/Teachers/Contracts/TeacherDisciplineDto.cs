using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Teachers
{
    public class TeacherDisciplineDto
    {
        [JsonPropertyName("disciplineId")]
        public string DisciplineId { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("metaData")]
        public List<string> MetaData { get; set; } = new();
    }
}