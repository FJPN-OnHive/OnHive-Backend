using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Payments;
using EHive.Payments.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Payments.Api.Endpoints
{
    public static class PaymentsEndpoints
    {
        public static WebApplication MapPaymentEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Payment/Providers", async (HttpContext context, [FromServices] IPaymentsService service) =>
            {
                var result = await service.GetProviders();
                if (result == null) return Results.Ok(Response<List<ProviderInfoDto>>.Empty());
                return Results.Ok(Response<List<ProviderInfoDto>>.Ok(result));
            })
           .WithName("GetProvidersInfo")
           .WithDescription("Get Payment Providers")
           .WithTags("Payments")
           .Produces<Response<List<ProviderInfoDto>>>()
           .WithOpenApi();

            app.MapPost("v1/Payment/Checkout", async (HttpContext context, [FromServices] IPaymentsService service, [FromBody] PaymentCheckoutDto paymentCheckout) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.Checkout(paymentCheckout, loggedUser);
                if (result == null) return Results.Ok(Response<PaymentReceiptDto>.Empty());
                return Results.Ok(Response<PaymentReceiptDto>.Ok(result));
            })
            .WithName("Checkout")
            .WithDescription("Payment Checkout")
            .WithTags("Payments")
            .Produces<Response<PaymentReceiptDto>>()
            .WithOpenApi();

            app.MapGet("v1/Payment/Cancel/{paymentId}", async (HttpContext context, [FromServices] IPaymentsService service, [FromRoute] string paymentId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.Cancel(paymentId, loggedUser);
                if (result == null) return Results.Ok(Response<PaymentReceiptDto>.Empty());
                return Results.Ok(Response<PaymentReceiptDto>.Ok(result));
            })
           .WithName("CancelPayment")
           .WithDescription("Cancel Payment")
           .WithTags("Payments")
           .Produces<Response<PaymentReceiptDto>>()
           .WithOpenApi();

            app.MapGet("v1/Payment/Receipt/{paymentId}", async (HttpContext context, [FromServices] IPaymentsService service, [FromRoute] string paymentId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetReceipt(paymentId, loggedUser);
                if (result == null) return Results.Ok(Response<PaymentReceiptDto>.Empty());
                return Results.Ok(Response<PaymentReceiptDto>.Ok(result));
            })
            .WithName("GetReceiptById")
            .WithDescription("Get Payment Receipt by Id")
            .WithTags("Payments")
            .Produces<Response<PaymentReceiptDto>>()
            .WithOpenApi();

            app.MapGet("v1/Payment/Receipts", async (HttpContext context, [FromServices] IPaymentsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetReceipts(loggedUser);
                if (result == null) return Results.Ok(Response<List<PaymentReceiptDto>>.Empty());
                return Results.Ok(Response<List<PaymentReceiptDto>>.Ok(result));
            })
              .WithName("GetReceipts")
              .WithDescription("Get Payment Receipts")
              .WithTags("Payments")
              .Produces<Response<List<PaymentReceiptDto>>>()
              .WithOpenApi();

            app.MapGet("v1/Payment/Receipt/Order/{orderId}", async (HttpContext context, [FromServices] IPaymentsService service, [FromRoute] string orderId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetReceiptByOrder(orderId, loggedUser);
                if (result == null) return Results.Ok(Response<PaymentReceiptDto>.Empty());
                return Results.Ok(Response<PaymentReceiptDto>.Ok(result));
            })
              .WithName("GetReceiptByOrder")
              .WithDescription("Get Payment Receipt by order")
              .WithTags("Payments")
              .Produces<Response<PaymentReceiptDto>>()
              .WithOpenApi();

            return app;
        }
    }
}