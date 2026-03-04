using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Catalog
{
    public class UserCouponDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("couponId")]
        public string CouponId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}