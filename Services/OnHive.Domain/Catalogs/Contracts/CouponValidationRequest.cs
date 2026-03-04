using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Catalog
{
    public class CouponValidationRequest
    {
        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("coupon")]
        [Required]
        public string Coupon { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        [Required]
        public string ProductId { get; set; } = string.Empty;
    }
}