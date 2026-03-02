using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Extensions;
using EHive.Users.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Users.Api.Endpoints
{
    internal static class RolesEndpoints
    {
        internal static WebApplication MapRolesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Role/{RoleId}", async (HttpContext context, [FromServices] IRolesService service, [FromRoute] string roleId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(roleId);
                if (result == null) return Results.Ok(Response<RoleDto>.Empty());
                return Results.Ok(Response<RoleDto>.Ok(result));
            })
            .WithName("GetRoleById")
            .WithDescription("Get Role by Id")
            .WithTags("Roles")
            .WithMetadata(PermissionConfig.Create("roles_read"))
            .Produces<Response<RoleDto>>();

            app.MapGet("v1/Roles", async (HttpContext context, [FromServices] IRolesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<RoleDto>>.Ok(result));
            })
            .WithName("GetRoles")
            .WithDescription("Get all Roles for the tenant")
            .WithTags("Roles")
            .WithMetadata(PermissionConfig.Create("roles_read"))
            .Produces<Response<PaginatedResult<RoleDto>>>();

            app.MapPost("v1/Role", async (HttpContext context, [FromServices] IRolesService service, [FromBody] RoleDto roleDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(roleDto, loggedUser);
                if (result == null) return Results.Ok(Response<RoleDto>.Empty());
                return Results.Ok(Response<RoleDto>.Ok(result));
            })
            .WithName("CreateRole")
            .WithDescription("Create a Role")
            .WithTags("Roles")
            .WithMetadata(PermissionConfig.Create("roles_create"))
            .Produces<Response<RoleDto>>();

            app.MapPut("v1/Role", async (HttpContext context, [FromServices] IRolesService service, [FromBody] RoleDto roleDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(roleDto, loggedUser);
                if (result == null) return Results.Ok(Response<RoleDto>.Empty());
                return Results.Ok(Response<RoleDto>.Ok(result));
            })
            .WithName("UpdateRole")
            .WithDescription("Update a Role by entity (full replace)")
            .WithTags("Roles")
            .WithMetadata(PermissionConfig.Create("roles_update"))
            .Produces<Response<RoleDto>>();

            app.MapPatch("v1/Role", async (HttpContext context, [FromServices] IRolesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.PatchAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<RoleDto>.Empty());
                return Results.Ok(Response<RoleDto>.Ok(result));
            })
            .WithName("PatchRole")
            .WithDescription("Update a Role by patch")
            .WithTags("Roles")
            .WithMetadata(PermissionConfig.Create("roles_update"))
            .Produces<Response<RoleDto>>();

            app.MapGet("v1/Permissions", async (HttpContext context, [FromServices] IRolesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetPermissions();
                if (result == null) return Results.Ok(Response<List<string>>.Empty());
                return Results.Ok(Response<List<string>>.Ok(result));
            })
            .WithName("GetPermissions")
            .WithDescription("Get all permissions")
            .WithTags("Permissions")
            .WithMetadata(PermissionConfig.Create("roles_read"))
            .Produces<Response<List<string>>>();

            return app;
        }
    }
}