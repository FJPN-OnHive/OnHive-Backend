using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Tenants.Api.Endpoints
{
    internal static class TenantThemesEndpoints
    {
        internal static WebApplication MapTenantThemesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Theme/{tenantId}/{domain}", async (HttpContext context, [FromServices] ITenantThemesService service, [FromRoute] string tenantId, [FromRoute] string domain) =>
            {
                var result = await service.GetCurrentByDomain(domain, tenantId);
                if (result == null) return Results.Ok(Response<TenantThemeDto>.Empty());
                return Results.Ok(Response<TenantThemeDto>.Ok(result));
            })
           .WithName("GetTenantThemeByDomain")
           .WithDescription("Get Tenant Theme By Domain")
           .WithTags("TenantThemes")
           .AllowAnonymous()
           .Produces<Response<TenantThemeDto>>();

            app.MapGet("v1/Theme/{TenantParameterId}", async (HttpContext context, [FromServices] ITenantThemesService service, [FromRoute] string tenantParameterId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(tenantParameterId);
                if (result == null) return Results.Ok(Response<TenantThemeDto>.Empty());
                return Results.Ok(Response<TenantThemeDto>.Ok(result));
            })
            .WithName("GetTenantThemeById")
            .WithDescription("Get Tenant Theme By Id")
            .WithTags("TenantThemes")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_read"))
            .Produces<Response<TenantThemeDto>>();

            app.MapGet("v1/Themes", async (HttpContext context, [FromServices] ITenantThemesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<TenantThemeDto>>.Ok(result));
            })
            .WithName("GetTenantTheme")
            .WithDescription("Get all Tenant Theme")
            .WithTags("TenantThemes")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_read"))
            .Produces<Response<PaginatedResult<TenantThemeDto>>>();

            app.MapPost("v1/Theme", async (HttpContext context, [FromServices] ITenantThemesService service, [FromBody] TenantThemeDto tenantThemeDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(tenantThemeDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantThemeDto>.Empty());
                return Results.Ok(Response<TenantThemeDto>.Ok(result));
            })
            .WithName("CreateTenantTheme")
            .WithDescription("Create a Tenant Theme")
            .WithTags("TenantThemes")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_create"))
            .Produces<Response<TenantThemeDto>>();

            app.MapPut("v1/Theme", async (HttpContext context, [FromServices] ITenantThemesService service, [FromBody] TenantThemeDto tenantThemeDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(tenantThemeDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantThemeDto>.Empty());
                return Results.Ok(Response<TenantThemeDto>.Ok(result));
            })
            .WithName("UpdateTenantTheme")
            .WithDescription("Update a Tenant Theme")
            .WithTags("TenantThemes")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_update"))
            .Produces<Response<TenantThemeDto>>();

            app.MapPatch("v1/Theme", async (HttpContext context, [FromServices] ITenantThemesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.PatchAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantThemeDto>.Empty());
                return Results.Ok(Response<TenantThemeDto>.Ok(result));
            })
            .WithName("PatchTenantTheme")
            .WithDescription("Patch a Tenant Theme")
            .WithTags("TenantThemes")
            .WithMetadata(PermissionConfig.Create("tenants_parameters_update"))
            .Produces<Response<TenantThemeDto>>();

            return app;
        }
    }
}