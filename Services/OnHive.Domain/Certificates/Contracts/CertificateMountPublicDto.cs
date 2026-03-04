using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Certificates
{
    public class CertificateMountPublicDto
    {
        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [JsonPropertyName("emissionDate")]
        public DateTime EmissionDate { get; set; }

        [JsonPropertyName("certificateKey")]
        public string CertificateKey { get; set; } = string.Empty;

        [JsonPropertyName("validationUrl")]
        public string ValidationUrl { get; set; } = string.Empty;
    }
}