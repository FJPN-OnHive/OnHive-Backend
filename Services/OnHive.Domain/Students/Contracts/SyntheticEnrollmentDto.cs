using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Students
{
    public class SyntheticEnrollmentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("productSku")]
        public string ProductSku { get; set; } = string.Empty;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("enrollments")]
        public long Enrollments { get; set; } = 0;

        [JsonPropertyName("certificates")]
        public long Certificates { get; set; } = 0;
    }
}