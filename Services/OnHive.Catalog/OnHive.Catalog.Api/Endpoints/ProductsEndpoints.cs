using OnHive.Authorization.Library.Extensions;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Catalog.Domain.Models;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Enums.Common;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace OnHive.Catalog.Api.Endpoints
{
    internal static class ProductsEndpoints
    {
        internal static WebApplication MapProductsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Product/{ProductId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string productId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(productId, loggedUser);
                if (result == null) return Results.Ok(Response<ProductDto>.Empty());
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("GetProductById")
            .WithDescription("Get Product by Id")
            .WithTags("Products")
            .WithMetadata(PermissionConfig.Create("products_read"))
            .Produces<Response<ProductDto>>();

            //app.MapGet("v1/Internal/Product/{ProductId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string productId) =>
            //{
            //    var result = await service.GetByIdAsync(productId);
            //    if (result == null) return Results.Ok(Response<ProductDto>.Empty());
            //    return Results.Ok(Response<ProductDto>.Ok(result));
            //})
            //.WithName("GetProductByIdInternal")
            //.WithDescription("Get Product by Id Internal")
            //.WithTags("Internal")
            //.Produces<Response<ProductDto>>()
            //.AllowAnonymous();

            // app.MapPost("v1/Internal/Products", async (HttpContext context, [FromServices] IProductsService service, [FromBody] List<string> productIds) =>
            // {
            //     var result = await service.GetByIdsAsync(productIds);
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            //.WithName("GetProductsByIdsInternal")
            //.WithDescription("Get Products by Ids Internal")
            //.WithTags("Internal")
            //.Produces<List<ProductDto>>()
            //.AllowAnonymous();

            // app.MapPost("v1/Internal/Products/Itens", async (HttpContext context, [FromServices] IProductsService service, [FromBody] List<string> itensIds) =>
            // {
            //     var result = await service.GetByItemIdsAsync(itensIds);
            //     if (result == null) return Results.NotFound();
            //     return Results.Ok(result);
            // })
            //.WithName("GetProductsByItensIdsInternal")
            //.WithDescription("Get Products by Itens Ids Internal")
            //.WithTags("Internal")
            //.Produces<List<ProductDto>>()
            //.AllowAnonymous();

            app.MapGet("v1/Product/Slug/{slug}/{tenantId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string slug, [FromRoute] string tenantId) =>
            {
                var result = await service.GetBySlugAsync(slug, tenantId);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
             .WithName("GetProductBySlugOpen")
             .WithDescription("Get Product by Slug")
             .WithTags("Products")
             .Produces<Response<ProductDto>>()
             .WithMetadata(PermissionConfig.Create("products_read"))
             .AllowAnonymous();

            app.MapGet("v1/Product/Sku/{sku}/{tenantId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string sku, [FromRoute] string tenantId) =>
            {
                var result = await service.GetBySkuAsync(sku, tenantId);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("GetProductBySkuOpen")
            .WithDescription("Get Product by Sku")
            .WithTags("Products")
            .Produces<Response<ProductDto>>()
            .AllowAnonymous();

            app.MapGet("v1/Product/Sku/{sku}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string sku) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetBySkuAsync(sku, loggedUser);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("GetProductBySku")
            .WithDescription("Get Product by Sku")
            .WithTags("Products")
            .Produces<Response<ProductDto>>()
            .AllowAnonymous();

            app.MapGet("v1/Products/{tenantId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<ProductDto>>.Ok(result));
            })
            .WithName("GetProductsOpen")
            .WithDescription("Get all Products")
            .WithTags("Products")
            .Produces<Response<PaginatedResult<ProductDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Products/Resume/{tenantId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetResumeByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<ProductDto>>.Ok(result));
            })
            .WithName("GetProductsResumeOpen")
            .WithDescription("Get all Products resume")
            .WithTags("Products")
            .Produces<Response<PaginatedResult<ProductDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Products/FilterScope/{tenantId}", async (HttpContext context, [FromServices] IProductsService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetFilterScopeAsync(tenantId);
                return Results.Ok(Response<FilterScope>.Ok(result));
            })
            .WithName("GetProducts Filter Scope")
            .WithDescription("Get filter scope")
            .WithTags("Products")
            .Produces<Response<FilterScope>>()
            .AllowAnonymous();

            app.MapGet("v1/Products", async (HttpContext context, [FromServices] IProductsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<ProductDto>>.Ok(result));
            })
            .WithName("GetProducts")
            .WithDescription("Get all Products")
            .WithTags("Products")
            .Produces<Response<PaginatedResult<ProductDto>>>()
            .AllowAnonymous();

            app.MapPost("v1/Product", async (HttpContext context, [FromServices] IProductsService service, [FromBody] ProductDto productDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(productDto, loggedUser);
                if (result == null) return Results.Ok(Response<ProductDto>.Empty());
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("CreateProduct")
            .WithDescription("Create an Product")
            .WithTags("Products")
            .WithMetadata(PermissionConfig.Create("products_create"))
            .Produces<Response<ProductDto>>();

            app.MapPut("v1/Product", async (HttpContext context, [FromServices] IProductsService service, [FromBody] ProductDto productDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(productDto, loggedUser);
                if (result == null) return Results.Ok(Response<ProductDto>.Empty());
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("UpdateProduct")
            .WithDescription("Update an Product")
            .WithTags("Products")
            .WithMetadata(PermissionConfig.Create("products_update"))
            .Produces<Response<ProductDto>>();

            app.MapPatch("v1/Product", async (HttpContext context, [FromServices] IProductsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.PatchAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<ProductDto>.Empty());
                return Results.Ok(Response<ProductDto>.Ok(result));
            })
            .WithName("PatchProduct")
            .WithDescription("Patch an Product")
            .WithTags("Products")
            .WithMetadata(PermissionConfig.Create("products_update"))
            .Produces<Response<ProductDto>>();

            app.MapGet("v1/Product/RefreshPrices", async (HttpContext context, [FromServices] IProductsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                await service.RefreshPrices(loggedUser);
                return Results.Ok(Response<bool>.Ok(true));
            })
            .WithName("ProductsRefreshPrices")
            .WithDescription("Products Refresh Prices")
            .WithTags("Products")
            .WithMetadata(PermissionConfig.Create("products_update"))
            .Produces<Response<bool>>();

            app.MapGet("v1/Products/Export/{tenantId}", async (HttpContext context,
                                                                [FromServices] IProductsService service,
                                                                [FromServices] CatalogApiSettings settings,
                                                                [FromRoute] string tenantId,
                                                                [FromQuery] string format = "json",
                                                                [FromQuery] string activeOnly = "true") =>
            {
                if (activeOnly != "true")
                {
                    var loggedUser = context.GetLoggedUser();
                    if (loggedUser?.User == null) return Results.Unauthorized();
                    if (!loggedUser?.User.Permissions.Contains("products_update") ?? false) return Results.Unauthorized();
                }

                var exportFormat = format.ToLower().Trim() switch
                {
                    "xml" => ExportFormats.Xml,
                    "json" => ExportFormats.Json,
                    "googlexml" => ExportFormats.GoogleXml,
                    "googletsv" => ExportFormats.GoogleTsv,
                    _ => ExportFormats.Csv,
                };
                var result = await service.GetExportData(exportFormat, tenantId, activeOnly == "true");
                if (result == null) return Results.NotFound();
                return exportFormat switch
                {
                    ExportFormats.Json => Results.File(result, "text/json", "products.json"),
                    ExportFormats.Xml => Results.File(result, "text/xml", "products.xml"),
                    ExportFormats.GoogleXml => Results.File(result, "text/xml", "products.xml"),
                    ExportFormats.GoogleTsv => Results.File(result, "text/txt", "products.txt"),
                    _ => Results.File(result, "text/csv", "products.csv"),
                };
            })
           .WithName("ExportProducts")
           .WithDescription("Get Formated Data")
           .WithTags("Products")
           .AllowAnonymous()
           .Produces<Stream>();

            return app;
        }
    }
}