using EHive.Core.Library.Enums.Orders;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Orders
{
    public class OrderDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        [Required]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        [Required]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; }

        [JsonPropertyName("itens")]
        [Required]
        public List<OrderItemDto> Itens { get; set; } = new();

        [JsonPropertyName("placementDate")]
        public DateTime PlacementDate { get; set; }

        [JsonPropertyName("closingDate")]
        public DateTime ClosingDate { get; set; }

        [JsonPropertyName("cancellationDate")]
        public DateTime CancellationDate { get; set; }

        [JsonPropertyName("refoundDate")]
        public DateTime RefoundDate { get; set; }

        [JsonPropertyName("orderDiscount")]
        public double OrderDiscount { get; set; }

        [JsonPropertyName("totalValue")]
        [Required]
        public double TotalValue { get; set; }
    }

    public class OrderItemDto
    {
        [JsonPropertyName("sequence")]
        [Required]
        public int Sequence { get; set; } = 0;

        [JsonPropertyName("productId")]
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        [Required]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("productName")]
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("productDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        [JsonPropertyName("productType")]
        public string ProductType { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        [Required]
        public double Price { get; set; }

        [JsonPropertyName("quantity")]
        [Required]
        public int Quantity { get; set; }

        [JsonPropertyName("discount")]
        public double Discount { get; set; }

        [JsonPropertyName("totalPrice")]
        public double TotalPrice { get; set; }

        [JsonPropertyName("couponsIds")]
        public List<string> CouponsIds { get; set; } = new();

        [JsonPropertyName("vouchersIds")]
        public List<string> VouchersIds { get; set; } = new();

        [JsonPropertyName("priceReason")]
        public string PriceReason { get; set; } = string.Empty;
    }
}