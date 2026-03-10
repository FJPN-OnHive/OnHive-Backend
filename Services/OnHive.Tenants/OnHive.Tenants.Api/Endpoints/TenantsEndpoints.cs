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
    internal static class TenantsEndpoints
    {
        internal static WebApplication MapTenantsEndpoints(this WebApplication app)
        {
            //app.MapGet("v1/Internal/Tenant/{tenantId}", async (HttpContext context, [FromServices] ITenantsService service, [FromRoute] string tenantId) =>
            //{
            //    var result = await service.GetByIdAsync(tenantId);
            //    if (result == null) return Results.Ok(Response<TenantDto>.Empty());
            //    return Results.Ok(Response<TenantDto>.Ok(result));
            //})
            //.WithName("GetInternalTenantById")
            //.WithDescription("Get Tenant By Id")
            //.WithTags("Internal")
            //.Produces<Response<TenantDto>>()
            //.AllowAnonymous();

            app.MapGet("v1/Tenant/Subdomain/{subdomain}", async (HttpContext context, [FromServices] ITenantsService service, [FromRoute] string subdomain) =>
            {
                var result = await service.GetByDomainAsync(subdomain);
                if (result == null) return Results.Ok(string.Empty);
                return Results.Ok(result);
            })
            .WithName("GetTenantByDomain")
            .WithDescription("Get Tenant By Domain")
            .WithTags("Tenants")
            .Produces<string>()
            .AllowAnonymous();

            app.MapGet("v1/Tenant/List", async (HttpContext context, [FromServices] ITenantsService service) =>
            {
                var result = await service.GetAllOpenAsync();
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<List<TenantResumeDto>>.Ok(result));
            })
            .WithName("ListTenantsOpen")
            .WithDescription("ListTenants ")
            .WithTags("Tenants")
            .Produces<Response<List<TenantResumeDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Tenant/Slug/{slug}", async (HttpContext context, [FromServices] ITenantsService service, [FromRoute] string slug) =>
            {
                var result = await service.GetBySlugAsync(slug);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<TenantResumeDto>.Ok(result));
            })
            .WithName("GetTenantBySlug")
            .WithDescription("Get Tenant By Slug")
            .WithTags("Tenants")
            .Produces<Response<TenantResumeDto>>()
            .AllowAnonymous();

            app.MapGet("v1/Tenant/{tenantId}", async (HttpContext context, [FromServices] ITenantsService service, [FromRoute] string tenantId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(tenantId);
                if (result == null) return Results.Ok(Response<TenantDto>.Empty());
                return Results.Ok(Response<TenantDto>.Ok(result));
            })
            .WithName("GetTenantById")
            .WithDescription("Get Tenant By Id")
            .WithTags("Tenants")
            .WithMetadata(PermissionConfig.Create("tenants_read"))
            .Produces<Response<TenantDto>>();

            app.MapGet("v1/Tenants", async (HttpContext context, [FromServices] ITenantsService service) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<TenantDto>>.Ok(result));
            })
            .WithName("GetTenants")
            .WithDescription("Get all Tenants")
            .WithTags("Tenants")
            .WithMetadata(PermissionConfig.Create("tenants_read"))
            .Produces<Response<PaginatedResult<TenantDto>>>();

            app.MapPost("v1/Tenant/Setup", async (HttpContext context, [FromServices] ITenantsService service, [FromBody] TenantSetupDto tenantDto) =>
            {
                var result = await service.SetupTenantAsync(tenantDto);
                if (result == null) return Results.Ok(Response<TenantDto>.Empty());
                return Results.Ok(Response<TenantDto>.Ok(result));
            })
            .WithName("SetupTenant")
            .WithDescription("Initial tenant setup")
            .WithTags("Tenants")
            .AllowAnonymous()
            .Produces<Response<TenantDto>>();

            app.MapPost("v1/Tenant", async (HttpContext context, [FromServices] ITenantsService service, [FromBody] TenantDto tenantDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(tenantDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantDto>.Empty());
                return Results.Ok(Response<TenantDto>.Ok(result));
            })
            .WithName("CreateTenant")
            .WithDescription("Create a Tenant")
            .WithTags("Tenants")
            .WithMetadata(PermissionConfig.Create("tenants_create"))
            .Produces<Response<TenantDto>>();

            app.MapPut("v1/Tenant", async (HttpContext context, [FromServices] ITenantsService service, [FromBody] TenantDto tenantDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(tenantDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantDto>.Empty());
                return Results.Ok(Response<TenantDto>.Ok(result));
            })
            .WithName("UpdateTenant")
            .WithDescription("Update a Tenant")
            .WithTags("Tenants")
            .WithMetadata(PermissionConfig.Create("tenants_update"))
            .Produces<Response<TenantDto>>();

            app.MapPatch("v1/Tenant", async (HttpContext context, [FromServices] ITenantsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<TenantDto>.Empty());
                return Results.Ok(Response<TenantDto>.Ok(result));
            })
            .WithName("PatchTenant")
            .WithDescription("Patch a Tenant")
            .WithTags("Tenants")
            .WithMetadata(PermissionConfig.Create("tenants_update"))
            .Produces<Response<TenantDto>>();

            return app;
        }
    }
}