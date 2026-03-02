using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Users.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Users.Api.Endpoints
{
    internal static class UserGroupsEndpoints
    {
        internal static WebApplication MapUserGroupsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/UserGroup/{UserGroupId}", async (HttpContext context, [FromServices] IUserGroupsService service, [FromRoute] string userGroupId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(userGroupId);
                if (result == null) return Results.Ok(Response<UserGroupDto>.Empty());
                return Results.Ok(Response<UserGroupDto>.Ok(result));
            })
            .WithName("GetUserGroupById")
            .WithDescription("Get UserGroup by Id")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<UserGroupDto>>();

            app.MapGet("v1/UserGroups", async (HttpContext context, [FromServices] IUserGroupsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<UserGroupDto>>.Ok(result));
            })
            .WithName("GetUserGroups")
            .WithDescription("Get all UserGroups")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<PaginatedResult<UserGroupDto>>>();

            app.MapPost("v1/UserGroup", async (HttpContext context, [FromServices] IUserGroupsService service, [FromBody] UserGroupDto userGroupDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(userGroupDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserGroupDto>.Empty());
                return Results.Ok(Response<UserGroupDto>.Ok(result));
            })
            .WithName("CreateUserGroup")
            .WithDescription("Create an UserGroup")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_create"))
            .Produces<Response<UserGroupDto>>();

            app.MapPut("v1/UserGroup", async (HttpContext context, [FromServices] IUserGroupsService service, [FromBody] UserGroupDto userGroupDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(userGroupDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserGroupDto>.Empty());
                return Results.Ok(Response<UserGroupDto>.Ok(result));
            })
            .WithName("UpdateUserGroup")
            .WithDescription("Update an UserGroup")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserGroupDto>>();

            app.MapPatch("v1/UserGroup", async (HttpContext context, [FromServices] IUserGroupsService service, [FromBody] JsonDocument userGroupDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.PatchAsync(userGroupDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserGroupDto>.Empty());
                return Results.Ok(Response<UserGroupDto>.Ok(result));
            })
            .WithName("PatchUserGroup")
            .WithDescription("Patch an UserGroup")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserGroupDto>>();

            app.MapDelete("v1/UserGroup/{UserGroupId}", async (HttpContext context, [FromServices] IUserGroupsService service, [FromRoute] string userGroupId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(userGroupId, loggedUser);
                if (!result) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("DeleteUserGroup")
            .WithDescription("Delete an UserGroup")
            .WithTags("UserGroups")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<bool>>();

            return app;
        }
    }
}