using Microsoft.AspNetCore.Mvc;
using OnHive.Authorization.Library.Extensions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using OnHive.Configuration.Library.Models;
using System.Text.Json;

namespace OnHive.Invoices.Api.Endpoints
{
    public static class InvoicesEndpoints
    {
        public static WebApplication MapInvoicesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Invoice/{InvoiceId}", async (HttpContext context, [FromServices] IInvoicesService service, [FromRoute] string invoiceId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(invoiceId);
                if (result == null) return Results.Ok(Response<InvoiceDto>.Empty());
                return Results.Ok(Response<InvoiceDto>.Ok(result));
            })
            .WithName("GetInvoiceById")
            .WithDescription("Get Invoice by Id")
            .WithTags("Invoices")
            .WithMetadata(PermissionConfig.Create("invoices_read"))
            .Produces<Response<InvoiceDto>>();

            app.MapGet("v1/Invoices", async (HttpContext context, [FromServices] IInvoicesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<InvoiceDto>>.Ok(result));
            })
            .WithName("GetInvoices")
            .WithDescription("Get all Invoices")
            .WithTags("Invoices")
            .WithMetadata(PermissionConfig.Create("invoices_read"))
            .Produces<Response<PaginatedResult<InvoiceDto>>>();

            app.MapPost("v1/Invoice", async (HttpContext context, [FromServices] IInvoicesService service, [FromBody] InvoiceDto invoiceDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(invoiceDto, loggedUser);
                if (result == null) return Results.Ok(Response<InvoiceDto>.Empty());
                return Results.Ok(Response<InvoiceDto>.Ok(result));
            })
            .WithName("CreateInvoice")
            .WithDescription("Create an Invoice")
            .WithTags("Invoices")
            .WithMetadata(PermissionConfig.Create("invoices_create"))
            .Produces<Response<InvoiceDto>>();

            app.MapPut("v1/Invoice", async (HttpContext context, [FromServices] IInvoicesService service, [FromBody] InvoiceDto invoiceDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(invoiceDto, loggedUser);
                if (result == null) return Results.Ok(Response<InvoiceDto>.Empty());
                return Results.Ok(Response<InvoiceDto>.Ok(result));
            })
            .WithName("UpdateInvoice")
            .WithDescription("Update an Invoice")
            .WithTags("Invoices")
            .WithMetadata(PermissionConfig.Create("invoices_update"))
            .Produces<Response<InvoiceDto>>();

            app.MapPatch("v1/Invoice", async (HttpContext context, [FromServices] IInvoicesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<InvoiceDto>.Empty());
                return Results.Ok(Response<InvoiceDto>.Ok(result));
            })
            .WithName("PatchInvoice")
            .WithDescription("Patch an Invoice")
            .WithTags("Invoices")
            .WithMetadata(PermissionConfig.Create("invoices_update"))
            .Produces<Response<InvoiceDto>>();

            return app;
        }
    }
}