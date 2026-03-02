//TODO: Move to EHive.Core.Library Project
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Payments
{
    public class PaymentDto
    {        
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;
    }
}
