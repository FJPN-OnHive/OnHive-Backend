using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Extensions;
using EHive.Payments.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Payments.Api.Endpoints
{
    public static class BankSlipSettingsEndpoints
    {
        public static WebApplication MapBankSlipSettingsesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/BankSlipSettings/{bankSlipSettingsId}", async (HttpContext context, [FromServices] IBankSlipSettingsService service, [FromRoute] string bankSlipSettingsId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(bankSlipSettingsId);
                if (result == null) return Results.Ok(Response<BankSlipSettingsDto>.Empty());
                return Results.Ok(Response<BankSlipSettingsDto>.Ok(result));
            })
           .WithName("GetBankSlipSettingsById")
           .WithDescription("Get an BankSlipSettings by id")
           .WithTags("BankSlipSettings")
           .Produces<Response<BankSlipSettingsDto>>()
           .WithOpenApi();

            app.MapGet("v1/BankSlipSettings/Filter", async (HttpContext context, [FromServices] IBankSlipSettingsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<BankSlipSettingsDto>>.Ok(result));
            })
            .WithName("GetBankSlipSettingses")
            .WithDescription("Get all BankSlipSettings for the tenant")
            .WithTags("BankSlipSettings")
            .Produces<Response<PaginatedResult<BankSlipSettingsDto>>>()
            .WithOpenApi();

            app.MapPost("v1/BankSlipSettings", async (HttpContext context, [FromServices] IBankSlipSettingsService service, [FromBody] BankSlipSettingsDto bankSlipSettingsDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(bankSlipSettingsDto, loggedUser);
                if (result == null) return Results.Ok(Response<BankSlipSettingsDto>.Empty());
                return Results.Ok(Response<BankSlipSettingsDto>.Ok(result));
            })
            .WithName("CreateBankSlipSettings")
            .WithDescription("Create an BankSlipSettings")
            .WithTags("BankSlipSettings")
            .Produces<Response<BankSlipSettingsDto>>()
            .WithOpenApi();

            app.MapPut("v1/BankSlipSettings", async (HttpContext context, [FromServices] IBankSlipSettingsService service, [FromBody] BankSlipSettingsDto bankSlipSettingsDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(bankSlipSettingsDto, loggedUser);
                if (result == null) return Results.Ok(Response<BankSlipSettingsDto>.Empty());
                return Results.Ok(Response<BankSlipSettingsDto>.Ok(result));
            })
            .WithName("UpdateBankSlipSettings")
            .WithDescription("Update a BankSlipSettings by entity (full replace)")
            .WithTags("BankSlipSettings")
            .Produces<Response<BankSlipSettingsDto>>()
            .WithOpenApi();

            app.MapPatch("v1/BankSlipSettings", async (HttpContext context, [FromServices] IBankSlipSettingsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<BankSlipSettingsDto>.Empty());
                return Results.Ok(Response<BankSlipSettingsDto>.Ok(result));
            })
            .WithName("PatchBankSlipSettings")
            .WithDescription("Update a BankSlipSettings by patch")
            .WithTags("BankSlipSettings")
            .Produces<Response<BankSlipSettingsDto>>()
            .WithOpenApi();

            return app;
        }
    }
}