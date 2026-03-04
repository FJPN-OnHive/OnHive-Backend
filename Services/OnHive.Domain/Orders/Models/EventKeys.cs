namespace OnHive.Invoices.Domain.Models
{
    public static class EventKeys
    {
        public const string CartCreated = "CartCreated";
        public const string CartAbandoned = "CartAbandoned";
        public const string CartItemAdded = "CartItemAdded";
        public const string CartItemRemoved = "CartItemRemoved";
        public const string CartView = "CartView";
        public const string OrderCreated = "OrderCreated";
        public const string OrderAbandoned = "OrderAbandoned";
        public const string OrderItemAdded = "OrderItemAdded";
        public const string OrderItemRemoved = "OrderItemRemoved";
        public const string OrderView = "OrderView";
    }
}