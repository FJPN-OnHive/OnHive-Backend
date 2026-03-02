using EHive.Core.Library.Enums.Catalog;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Catalog
{
    public class CouponValidationResponse
    {
        [JsonPropertyName("IsValid")]
        public bool IsValid { get; set; } = true;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("couponId")]
        public string CouponId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public CouponValidationResponseCodes Code { get; set; }
    }
}