using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Orders.Api.Endpoints
{
    public static class OrdersEndpoints
    {
        public static WebApplication MapOrdersEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Order/{OrderId}", async (HttpContext context, [FromServices] IOrdersService service, [FromRoute] string orderId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(orderId, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());

                return Results.Ok(Response<OrderDto>.Ok(result));
            })
            .WithName("GetOrderById")
            .WithDescription("Get Order by Id")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_read"))
            .Produces<Response<OrderDto>>();

            app.MapGet("v1/Orders", async (HttpContext context, [FromServices] IOrdersService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<OrderDto>>.Ok(result));
            })
            .WithName("GetOrders")
            .WithDescription("Get all Orders")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_read"))
            .Produces<Response<PaginatedResult<OrderDto>>>();

            app.MapPost("v1/Order", async (HttpContext context, [FromServices] IOrdersService service, [FromBody] OrderDto orderDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(orderDto, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
            .WithName("CreateOrder")
            .WithDescription("Create an Order")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_create"))
            .Produces<Response<OrderDto>>();

            app.MapPut("v1/Order", async (HttpContext context, [FromServices] IOrdersService service, [FromBody] OrderDto orderDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(orderDto, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
            .WithName("UpdateOrder")
            .WithDescription("Update an Order")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_update"))
            .Produces<Response<OrderDto>>();

            app.MapPatch("v1/Order", async (HttpContext context, [FromServices] IOrdersService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
            .WithName("PatchOrder")
            .WithDescription("Patch an Order")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_update"))
            .Produces<Response<OrderDto>>();

            app.MapPost("v1/Order/FromCart/{cartId}", async (HttpContext context, [FromServices] IOrdersService service, [FromRoute] string cartId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.FromCart(cartId, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
           .WithName("CreateOrderFromCart")
           .WithDescription("Create Order From Cart")
           .WithTags("Orders")
           .WithMetadata(PermissionConfig.Create("orders_create"))
           .Produces<Response<OrderDto>>();

            app.MapPut("v1/Order/Cancel/{orderId}", async (HttpContext context, [FromServices] IOrdersService service, [FromRoute] string orderId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.Cancel(orderId, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
           .WithName("CancelOrder")
           .WithDescription("Cancel Order")
           .WithTags("Orders")
           .WithMetadata(PermissionConfig.Create("orders_update"))
           .Produces<Response<OrderDto>>();

            app.MapPut("v1/Order/RequestRefound/{orderId}", async (HttpContext context, [FromServices] IOrdersService service, [FromRoute] string orderId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.RequestRefound(orderId, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
           .WithName("RequestRefoundOfOrder")
           .WithDescription("Request Refound Of Order")
           .WithTags("Orders")
           .WithMetadata(PermissionConfig.Create("orders_update"))
           .Produces<Response<OrderDto>>();

            app.MapGet("v1/Order/Initialize/{sku}", async (HttpContext context, [FromServices] IOrdersService service, [FromRoute] string sku) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.Initialize(sku, loggedUser);
                if (result == null) return Results.Ok(Response<OrderDto>.Empty());
                return Results.Ok(Response<OrderDto>.Ok(result));
            })
            .WithName("InitializeOrderBySku")
            .WithDescription("Initialize a Order by product sku")
            .WithTags("Orders")
            .WithMetadata(PermissionConfig.Create("orders_create"))
            .Produces<Response<OrderDto>>();

            return app;
        }
    }
}