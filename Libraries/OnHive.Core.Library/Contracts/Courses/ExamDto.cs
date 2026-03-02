using EHive.Core.Library.Abstractions.Enrich;
using EHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Courses
{
    public class ExamDto : IEnrichable
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1";

        [JsonPropertyName("versionNumber")]
        public int VersionNumber { get; set; } = 1;

        [JsonPropertyName("activeVersion")]
        public bool ActiveVersion { get; set; } = true;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("questions")]
        public List<ExamQuestionDto> Questions { get; set; } = new();

        [JsonPropertyName("requiredLessons")]
        public List<string> RequiredLessons { get; set; } = new();

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("totalScore")]
        public double TotalScore { get; set; } = 0.0;

        [JsonPropertyName("requiredScore")]
        public double RequiredScore { get; set; } = 0.0;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("metaData")]
        public List<string> MetaData { get; set; } = new();

        [JsonPropertyName("maxRetries")]
        public int MaxRetries { get; set; } = 1;

        [JsonPropertyName("customAttributes")]
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }
}