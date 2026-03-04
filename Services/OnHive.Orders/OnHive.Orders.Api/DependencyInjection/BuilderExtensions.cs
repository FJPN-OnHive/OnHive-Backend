using OnHive.Configuration.Library.Extensions;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Events.Api.DependencyInjection;
using OnHive.Invoices.Domain.Models;
using OnHive.Orders.Domain.Models;

namespace OnHive.Orders.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureOrdersApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<OrdersApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.ConfigureEventRegister("Orders");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CartCreated,
                Message = "Event triggered when an cart is created",
                Origin = "Carts",
                Tags = new List<string> { "Cart", "Created" },
                Fields = new Dictionary<string, string> {
                    { "CartId", "The id of the Cart" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Cart" },
                    { "ItemName", "Cart Item name" },
                    { "ItemId", "Cart Item Id" },
                    { "ItemCode", "Cart Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CartView,
                Message = "Event triggered when an cart is viewed",
                Origin = "Carts",
                Tags = new List<string> { "Cart", "Viewed" },
                Fields = new Dictionary<string, string> {
                    { "CartId", "The id of the Cart" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Cart" },
                    { "ItemName", "Cart Item name" },
                    { "ItemId", "Cart Item Id" },
                    { "ItemCode", "Cart Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CartAbandoned,
                Message = "Event triggered when an cart is Abandoned",
                Origin = "Carts",
                Tags = new List<string> { "Cart", "Abandoned" },
                Fields = new Dictionary<string, string> {
                    { "CartId", "The id of the Cart" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Cart" },
                    { "ItemName", "Cart Item name" },
                    { "ItemId", "Cart Item Id" },
                    { "ItemCode", "Cart Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CartItemAdded,
                Message = "Event triggered when an item is added to a cart",
                Origin = "Carts",
                Tags = new List<string> { "Cart", "Added" },
                Fields = new Dictionary<string, string> {
                    { "CartId", "The id of the Cart" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Cart" },
                    { "ItemName", "Cart Item name" },
                    { "ItemId", "Cart Item Id" },
                    { "ItemCode", "Cart Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CartItemRemoved,
                Message = "Event triggered when an item is removed from a cart",
                Origin = "Carts",
                Tags = new List<string> { "Cart", "Removed" },
                Fields = new Dictionary<string, string> {
                    { "CartId", "The id of the Cart" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Cart" },
                    { "ItemName", "Cart Item name" },
                    { "ItemId", "Cart Item Id" },
                    { "ItemCode", "Cart Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.OrderCreated,
                Message = "Event triggered when an Order is created",
                Origin = "Orders",
                Tags = new List<string> { "Order", "Created" },
                Fields = new Dictionary<string, string> {
                    { "OrderId", "The id of the Order" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Order" },
                    { "ItemName", "Order Item name" },
                    { "ItemId", "Order Item Id" },
                    { "ItemCode", "Order Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.OrderView,
                Message = "Event triggered when an Order is viewed",
                Origin = "Orders",
                Tags = new List<string> { "Order", "Viewed" },
                Fields = new Dictionary<string, string> {
                    { "OrderId", "The id of the Order" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Order" },
                    { "ItemName", "Order Item name" },
                    { "ItemId", "Order Item Id" },
                    { "ItemCode", "Order Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.OrderAbandoned,
                Message = "Event triggered when an Order is Abandoned",
                Origin = "Orders",
                Tags = new List<string> { "Order", "Abandoned" },
                Fields = new Dictionary<string, string> {
                    { "OrderId", "The id of the Order" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Order" },
                    { "ItemName", "Order Item name" },
                    { "ItemId", "Order Item Id" },
                    { "ItemCode", "Order Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.OrderItemAdded,
                Message = "Event triggered when an item is added to a Order",
                Origin = "Orders",
                Tags = new List<string> { "Order", "Added" },
                Fields = new Dictionary<string, string> {
                    { "OrderId", "The id of the Order" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Order" },
                    { "ItemName", "Order Item name" },
                    { "ItemId", "Order Item Id" },
                    { "ItemCode", "Order Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.OrderItemRemoved,
                Message = "Event triggered when an item is removed from a Order",
                Origin = "Orders",
                Tags = new List<string> { "Order", "Removed" },
                Fields = new Dictionary<string, string> {
                    { "OrderId", "The id of the Order" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the Order" },
                    { "ItemName", "Order Item name" },
                    { "ItemId", "Order Item Id" },
                    { "ItemCode", "Order Item Code" }
                }
            });

            return builder;
        }
    }
}