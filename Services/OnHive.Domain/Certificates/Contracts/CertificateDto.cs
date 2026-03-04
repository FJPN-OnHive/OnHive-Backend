using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Certificates
{
    public class CertificateDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("templateUrl")]
        public string TemplateUrl { get; set; } = string.Empty;

        [JsonPropertyName("templateBody")]
        public string TemplateBody { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public List<CertificateParameterDto> Parameters { get; set; } = new();
    }
}