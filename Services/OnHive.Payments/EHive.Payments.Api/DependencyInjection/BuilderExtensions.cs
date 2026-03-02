using EHive.Configuration.Library.Extensions;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Api.DependencyInjection;
using EHive.Payments.Domain.Models;

namespace EHive.Payments.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigurePaymentsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<PaymentsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.ConfigureEventRegister("Payments");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.Checkout,
                Message = "Event triggered when an Checkout is triggered",
                Origin = "Payments",
                Tags = new List<string> { "Payment", "Created" },
                Fields = new Dictionary<string, string> {
                    { "PaymentId", "The id of the payment" },
                    { "PaymentExternalId", "The external id of the payment" },
                    { "PaymentCode", "The code of the payment" },
                    { "PaymentStatus", "Payment Status" },
                    { "OrderId", "The id of the order" },
                    { "PaymentType", "The Type of the payment" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the invoice" },
                    { "ItemName", "Invoice Item name" },
                    { "ItemId", "Invoice Item Id" },
                    { "ItemCode", "Invoice Item Code" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.PaymentCanceled,
                Message = "Event triggered when an payment is canceled",
                Origin = "Payments",
                Tags = new List<string> { "Payment", "Canceled" },
                Fields = new Dictionary<string, string>
                {
                    { "PaymentId", "The id of the payment" },
                    { "PaymentExternalId", "The external id of the payment" },
                    { "PaymentCode", "The code of the payment" },
                    { "PaymentStatus", "Payment Status" },
                    { "OrderId", "The id of the order" },
                    { "PaymentType", "The Type of the payment" },
                    { "ClientId", "The code of the client" },
                    { "ClientName", "The name of the client" },
                    { "ClientEmail", "The Email of the client" },
                    { "Value", "The value of the invoice" },
                    { "ItemName", "Invoice Item name" },
                    { "ItemId", "Invoice Item Id" },
                    { "ItemCode", "Invoice Item Code" }
                }
            });

            return builder;
        }
    }
}