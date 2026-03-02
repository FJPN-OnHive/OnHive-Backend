using EHive.Core.Library.Enums.Orders;

namespace EHive.Core.Library.Entities.Orders
{
    public class Order : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string PaymentId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public OrderStatus Status { get; set; }

        public List<OrderItem> Itens { get; set; } = new();

        public DateTime PlacementDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public DateTime CancellationDate { get; set; }

        public DateTime RefoundDate { get; set; }

        public double OrderDiscount { get; set; }

        public double TotalValue { get; set; }
    }

    public class OrderItem
    {
        public int Sequence { get; set; } = 0;

        public string ProductId { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public string ProductType { get; set; } = string.Empty;

        public double Price { get; set; }

        public int Quantity { get; set; }

        public double Discount { get; set; }

        public double TotalPrice { get; set; }

        public List<string> CouponsIds { get; set; } = new();

        public List<string> VouchersIds { get; set; } = new();

        public string PriceReason { get; set; } = string.Empty;
    }
}