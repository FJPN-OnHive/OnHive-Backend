namespace OnHive.Core.Library.Entities.Orders
{
    public class Cart : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public DateTime PlacementDate { get; set; }

        public List<CartItem> Itens { get; set; } = new();
    }

    public class CartItem
    {
        public int Sequence { get; set; } = 0;

        public string ExternalId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public double ProductValue { get; set; } = 0.0;

        public string ProductType { get; set; } = string.Empty;

        public int Quantity { get; set; } = 0;

        public string PriceReason { get; set; } = string.Empty;
    }
}