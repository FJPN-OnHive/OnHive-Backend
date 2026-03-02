using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Catalog
{
    public class CouponApplyRequest : CouponValidationRequest
    {
        [JsonPropertyName("orderId")]
        [Required]
        public string OrderId { get; set; } = string.Empty;
    }
}