using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Tenants.Api.Endpoints
{
    internal static class TenantParametersEndpoints
    {
        internal static WebApplication MapTenantParametersEndpoints(this WebApplication app)
        {
            // app.MapGet("v1/Internal/TenantParameter/{tenantId}/{group}/{key}", async (HttpContext context, [FromServices] ITenantParametersService service, [FromRoute] string tenantId, [FromRoute] string group, [FromRoute] string key) =>
            // {
            //     var result = await service.GetByKey(group, key, tenantId);
            //     if (result == null) return Results.Ok(Response<TenantParameterDto>.Empty());
            //     return Results.Ok(Response<TenantParameterDto>.Ok(result));
            // })
            //.WithName("GetTenantParameterByKey")
            //.WithDescription("Get Tenant Parameter By Key")
            //.WithTags("Internal")
            //.Produces<Response<TenantParameterDto>>()
            //.AllowAnonymous();

            // app.MapGet("v1/Internal/TenantParameters/{tenantId}/{group}", async (HttpContext context, [FromServices] ITenantParametersService service, [FromRoute] string tenantId, [FromRoute] string group) =>
            // {
            //     var result = await service.GetByGroup(group, tenantId);
            //     if (result == null || !result.Any()) return Results.Ok(Response<TenantParameterDto>.Empty());
            //     return Results.Ok(Response<IEnumerable<TenantParameterDto>>.Ok(result));
            // })
            //.WithName("GetTenantParameterByGroup")
            //.WithDescription("Get Tenant Parameter By Group")
            //.WithTags("Internal")
            //.Produces<Response<IEnumerable<TenantParameterDto>>>()
            //.AllowAnonymous();

            app.MapGet("v1/TenantParameter/{TenantParameterId}", async (HttpContext context, [FromServices] ITenantParametersService service, [FromRoute] string tenantParameterId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(tenantParameterId);
                if (result == null) return Results.Ok(Response<TenantParameterDto>.Empty());
                return Results.Ok(Response<TenantParameterDto>.Ok(result));
            })
            .WithName("GetTenantParameterById")
            .WithDescription("Get Tenant Parameter By Id")
            .WithTags("TenantParameters")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_read"))
            .Produces<Response<TenantParameterDto>>();

            app.MapGet("v1/TenantParameters", async (HttpContext context, [FromServices] ITenantParametersService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<TenantParameterDto>>.Ok(result));
            })
            .WithName("GetTenantParameters")
            .WithDescription("Get all Tenant Parameters")
            .WithTags("TenantParameters")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_read"))
            .Produces<Response<PaginatedResult<TenantParameterDto>>>();

            app.MapPost("v1/TenantParameter", async (HttpContext context, [FromServices] ITenantParametersService service, [FromBody] TenantParameterDto tenantParameterDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(tenantParameterDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantParameterDto>.Empty());
                return Results.Ok(Response<TenantParameterDto>.Ok(result));
            })
            .WithName("CreateTenantParameter")
            .WithDescription("Create a Tenant Parameter")
            .WithTags("TenantParameters")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_create"))
            .Produces<Response<TenantParameterDto>>();

            app.MapPut("v1/TenantParameter", async (HttpContext context, [FromServices] ITenantParametersService service, [FromBody] TenantParameterDto tenantParameterDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(tenantParameterDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantParameterDto>.Empty());
                return Results.Ok(Response<TenantParameterDto>.Ok(result));
            })
            .WithName("UpdateTenantParameter")
            .WithDescription("Update a Tenant Parameter")
            .WithTags("TenantParameters")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_update"))
            .Produces<Response<TenantParameterDto>>();

            app.MapPatch("v1/TenantParameter", async (HttpContext context, [FromServices] ITenantParametersService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantParameterDto>.Empty());
                return Results.Ok(Response<TenantParameterDto>.Ok(result));
            })
            .WithName("PatchTenantParameter")
            .WithDescription("Patch a Tenant Parameter")
            .WithTags("TenantParameters")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_update"))
            .Produces<Response<TenantParameterDto>>();

            return app;
        }
    }
}