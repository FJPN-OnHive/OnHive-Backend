namespace OnHive.Orders.Domain.Models
{
    public class OrdersApiSettings
    {
        public string? OrdersAdminPermission { get; set; } = "orders_admin";

        public string? OrderViewUrl { get; set; } = string.Empty;

        public string? OrderCreatedTemplate { get; set; }

        public string? OrderCheckoutTemplate { get; set; }

        public string InvoiceProvider { get; set; } = "Default";
    }
}