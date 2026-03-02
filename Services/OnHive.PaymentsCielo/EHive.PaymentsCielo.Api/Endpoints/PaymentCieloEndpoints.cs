using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Payments;
using EHive.PaymentsCielo.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace EHive.PaymentsCielo.Api.Endpoints
{
    public static class PaymentCieloEndpoints
    {
        public static WebApplication MapPaymentCieloEndpoints(this WebApplication app)
        {
            // app.MapGet("v1/Payment/ProviderInfo", async (HttpContext context, [FromServices] IPaymentCieloService service) =>
            // {
            //     var result = await service.GetProviderInfoAsync();
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            // .WithName("ProviderInfo")
            // .WithDescription("Get Provider info")
            // .WithTags("Internal")
            // .Produces<ProviderInfoDto>()
            // .AllowAnonymous()
            // .WithOpenApi();

            // app.MapPost("v1/Payment/Checkout", async (HttpContext context, [FromServices] IPaymentCieloService service, [FromBody] PaymentCheckoutDto paymentCheckout) =>
            // {
            //     var result = await service.CheckoutAsync(paymentCheckout);
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            //.WithName("Checkout")
            //.WithDescription("Execute Checkout")
            //.WithTags("Internal")
            //.Produces<PaymentReceiptDto>()
            //.AllowAnonymous()
            //.WithOpenApi();

            // app.MapPost("v1/Payment", async (HttpContext context, [FromServices] IPaymentCieloService service, [FromBody] PaymentReceiptDto receipt) =>
            // {
            //     var result = await service.GetPaymentAsync(receipt);
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            //.WithName("GetPayment")
            //.WithDescription("Get Payment Receipt")
            //.WithTags("Internal")
            //.Produces<PaymentReceiptDto>()
            //.AllowAnonymous()
            //.WithOpenApi();

            // app.MapGet("v1/Payment/Types", async (HttpContext context, [FromServices] IPaymentCieloService service) =>
            // {
            //     var result = await service.GetPaymentTypesAsync();
            //     return Results.Ok(result);
            // })
            //.WithName("PaymentTypes")
            //.WithDescription("Get Payment types")
            //.WithTags("Internal")
            //.Produces<List<PaymentTypeDto>>()
            //.AllowAnonymous()
            //.WithOpenApi();

            // app.MapPut("v1/Payment/Cancel", async (HttpContext context, [FromServices] IPaymentCieloService service, [FromBody] PaymentReceiptDto reciept) =>
            // {
            //     var result = await service.CancelPaymentAsync(reciept);
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            //.WithName("CancelPayment")
            //.WithDescription("Cancel Payment")
            //.WithTags("Internal")
            //.Produces<PaymentReceiptDto>()
            //.AllowAnonymous()
            //.WithOpenApi();

            return app;
        }
    }
}