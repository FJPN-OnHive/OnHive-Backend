using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Teachers
{
    public class TeacherDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("disciplines")]
        public List<TeacherDisciplineDto> Disciplines { get; set; } = new();

        [JsonPropertyName("metaData")]
        public List<string> MetaData { get; set; } = new();
    }
}