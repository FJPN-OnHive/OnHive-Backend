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
    public static class CartsEndpoints
    {
        public static WebApplication MapCartsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Cart/{CartId}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string cartId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(cartId);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("GetCartById")
            .WithDescription("Get Cart by Id")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_read"))
            .Produces<Response<CartDto>>();

            app.MapGet("v1/Carts", async (HttpContext context, [FromServices] ICartsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<CartDto>>.Ok(result));
            })
            .WithName("GetCarts")
            .WithDescription("Get all Carts")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_read"))
            .Produces<Response<PaginatedResult<CartDto>>>();

            app.MapPost("v1/Cart", async (HttpContext context, [FromServices] ICartsService service, [FromBody] CartDto cartDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(cartDto, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("CreateCart")
            .WithDescription("Create an Cart")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_create"))
            .Produces<Response<CartDto>>();

            app.MapPut("v1/Cart", async (HttpContext context, [FromServices] ICartsService service, [FromBody] CartDto cartDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(cartDto, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("UpdateCart")
            .WithDescription("Update an Cart")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_update"))
            .Produces<Response<CartDto>>();

            app.MapPatch("v1/Cart", async (HttpContext context, [FromServices] ICartsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("PatchCart")
            .WithDescription("Patch an Cart")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_update"))
            .Produces<Response<CartDto>>();

            app.MapGet("v1/Cart/Initialize/{sku}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string sku) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.Initialize(sku, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("InitializeCartBySku")
            .WithDescription("Initialize a Cart by product sku")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_create"))
            .Produces<Response<CartDto>>();

            app.MapPut("v1/Cart/AddProduct/{cartId}/{sku}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string cartId, [FromRoute] string sku) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.AddProduct(cartId, sku, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
            .WithName("AddProductToCart")
            .WithDescription("Add Product To a Cart")
            .WithTags("Carts")
            .WithMetadata(PermissionConfig.Create("carts_update"))
            .Produces<Response<CartDto>>();

            app.MapDelete("v1/Cart/RemoveProduct/{cartId}/{sequence}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string cartId, [FromRoute] int sequence) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.RemoveProduct(cartId, sequence, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
           .WithName("RemoveProductFromCart")
           .WithDescription("Remove Product From Cart")
           .WithTags("Carts")
           .WithMetadata(PermissionConfig.Create("carts_update"))
           .Produces<Response<CartDto>>();

            app.MapPut("v1/Cart/ChangeQuantity/{cartId}/{sequence}/{quantity}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string cartId, [FromRoute] int sequence, [FromRoute] int quantity) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.ChangeProductQuantity(cartId, sequence, quantity, loggedUser);
                if (result == null) return Results.Ok(Response<CartDto>.Empty());
                return Results.Ok(Response<CartDto>.Ok(result));
            })
           .WithName("ChangeProductQuantity")
           .WithDescription("Change Product Quantity")
           .WithTags("Carts")
           .WithMetadata(PermissionConfig.Create("carts_update"))
           .Produces<Response<CartDto>>();

            app.MapDelete("v1/Cart/{cartId}", async (HttpContext context, [FromServices] ICartsService service, [FromRoute] string cartId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                await service.Delete(cartId, loggedUser);
                return Results.Ok(Response<string>.Ok("deleted"));
            })
           .WithName("DeleteCart")
           .WithDescription("Delete Cart")
           .WithTags("Carts")
           .WithMetadata(PermissionConfig.Create("carts_update"))
           .Produces<Response<string>>();

            return app;
        }
    }
}