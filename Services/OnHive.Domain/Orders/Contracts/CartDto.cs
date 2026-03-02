using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Orders
{
    public class CartDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("placementDate")]
        public DateTime PlacementDate { get; set; }

        [JsonPropertyName("itens")]
        public List<CartItemDto> Itens { get; set; } = new();
    }

    public class CartItemDto
    {
        [JsonPropertyName("sequence")]
        public int Sequence { get; set; } = 0;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("productDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        [JsonPropertyName("productValue")]
        public double ProductValue { get; set; } = 0.0;

        [JsonPropertyName("productType")]
        public string ProductType { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 0;

        [JsonPropertyName("priceReason")]
        public string PriceReason { get; set; } = string.Empty;
    }
}