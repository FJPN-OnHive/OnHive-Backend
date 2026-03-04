using OnHive.Core.Library.Abstractions.Enrich;
using OnHive.Core.Library.Enums.Payments;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Catalog
{
    public class ProductDto : IEnrichable
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vId")]
        [MaxLength(256)]
        public string VId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        [MaxLength(100)]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("itemType")]
        [MaxLength(100)]
        public string ItemType { get; set; } = string.Empty;

        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("itemUrl")]
        public string ItemUrl { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imageAltText")]
        public string? ImageAltText { get; set; }

        [JsonPropertyName("externalId")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("item")]
        public object? Item { get; set; }

        [JsonPropertyName("fullPrice")]
        public double FullPrice { get; set; } = 0;

        [JsonPropertyName("lowPrice")]
        public double LowPrice { get; set; } = 0;

        [JsonPropertyName("prices")]
        public List<ProductPriceDto>? Prices { get; set; } = new();

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("relatedProducts")]
        public List<string> RelatedProducts { get; set; } = [];

        [JsonPropertyName("rate")]
        public int Rate { get; set; } = 0;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("alternativeSlugs")]
        public List<string>? AlternativeSlugs { get; set; }

        [JsonPropertyName("specialDiscount")]
        public SpecialDiscount SpecialDiscount { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("sells")]
        public int Sells { get; set; } = 0;

        [JsonPropertyName("sales")]
        public int Sales { get; set; } = 0;

        [JsonPropertyName("customAttributes")]
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }

    public class ProductPriceDto
    {
        [JsonPropertyName("paymentType")]
        public PaymentType PaymentType { get; set; } = PaymentType.CreditCard;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("sequence")]
        public int Sequence { get; set; } = 0;

        [JsonPropertyName("price")]
        public double Price { get; set; } = 0;

        [JsonPropertyName("discount")]
        public double Discount { get; set; } = 0;

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("installments")]
        public List<InstallmentDto> Installments { get; set; } = new();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("activePromo")]
        public bool ActivePromo { get; set; } = false;
    }

    public class InstallmentDto
    {
        [JsonPropertyName("installments")]
        public int Installments { get; set; } = 1;

        [JsonPropertyName("value")]
        public double Value { get; set; } = 0;

        [JsonPropertyName("interest")]
        public double Interest { get; set; } = 0;

        [JsonPropertyName("total")]
        public double Total { get; set; } = 0;
    }

    public class SpecialDiscount
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }
}