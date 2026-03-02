using EHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Catalog
{
    public class Product : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string ItemId { get; set; } = string.Empty;

        public string ItemUrl { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? ImageAltText { get; set; }

        public string? ExternalId { get; set; }

        public double FullPrice { get; set; } = 0;

        public double LowPrice { get; set; } = 0;

        public List<ProductPrice> Prices { get; set; } = new();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<string> Tags { get; set; } = new();

        public List<string> Categories { get; set; } = new();

        public List<string> RelatedProducts { get; set; } = [];

        public int Rate { get; set; } = 0;

        public string Slug { get; set; } = string.Empty;

        public List<string>? AlternativeSlugs { get; set; }

        public int Sells { get; set; } = 0;

        public int Sales { get; set; } = 0;
    }

    public class ProductPrice
    {
        public PaymentType PaymentType { get; set; } = PaymentType.CreditCard;

        public string Description { get; set; } = string.Empty;

        public int Sequence { get; set; } = 0;

        public string ExternalId { get; set; } = string.Empty;

        public double Price { get; set; } = 0;

        public double Discount { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Installment> Installments { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        public List<string> Categories { get; set; } = new();

        public bool ActivePromo { get; set; } = false;
    }

    public class Installment
    {
        public int Installments { get; set; } = 1;

        public double Value { get; set; } = 0;

        public double Interest { get; set; } = 0;

        public double Total { get; set; } = 0;
    }
}