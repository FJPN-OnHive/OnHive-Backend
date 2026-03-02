using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Enums.Users;
using EHive.Users.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Users.Api.Endpoints
{
    internal static class UserProfilesEndpoints
    {
        internal static WebApplication MapUserProfilesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/UserProfile/{UserProfileId}", async (HttpContext context, [FromServices] IUserProfilesService service, [FromRoute] string userProfileId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(userProfileId);
                if (result == null) return Results.Ok(Response<UserProfileDto>.Empty());
                return Results.Ok(Response<UserProfileDto>.Ok(result));
            })
            .WithName("GetUserProfileById")
            .WithDescription("Get UserProfile by Id")
            .WithTags("UserProfiles")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<UserProfileDto>>();

            app.MapGet("v1/UserProfiles/ByUser/{UserId}", async (HttpContext context, [FromServices] IUserProfilesService service, [FromRoute] string userId) =>
            {
                var result = await service.GetByUserIdAsync(userId);
                if (result == null) return Results.Ok(Response<List<UserProfileCompleteDto>>.Empty());
                return Results.Ok(Response<List<UserProfileCompleteDto>>.Ok(result));
            })
            .WithName("GetUserProfilesByUserId")
            .WithDescription("Get UserProfile by UserId")
            .WithTags("UserProfiles")
            .Produces<Response<List<UserProfileCompleteDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/UserProfiles/Management/ByUser/{UserId}", async (HttpContext context, [FromServices] IUserProfilesService service, [FromRoute] string userId) =>
            {
                var result = await service.GetByUserIdManagementAsync(userId);
                if (result == null) return Results.Ok(Response<List<UserProfileDto>>.Empty());
                return Results.Ok(Response<List<UserProfileDto>>.Ok(result));
            })
            .WithName("GetUserProfilesMaganegementByUserId")
            .WithDescription("Get UserProfile by UserId")
            .WithTags("UserProfiles")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<List<UserProfileDto>>>();

            app.MapGet("v1/UserProfiles/{TenantId}", async (HttpContext context, [FromServices] IUserProfilesService service, [FromRoute] string tenantId, [FromQuery] string profileType = "") =>
            {
                var filter = context.GetFilter();
                var type = profileType.ToLower() switch
                {
                    "staff" => ProfileTypes.Staff,
                    "student" => ProfileTypes.Student,
                    "teacher" => ProfileTypes.Teacher,
                    "monitor" => ProfileTypes.Monitor,
                    "author" => ProfileTypes.Author,
                    _ => ProfileTypes.None
                };
                var result = await service.GetByTypeAsync(filter, type, tenantId);
                return Results.Ok(Response<PaginatedResult<UserProfileCompleteDto>>.Ok(result));
            })
            .WithName("GetUserProfilesByType")
            .WithDescription("Get all UserProfiles")
            .WithTags("UserProfiles")
            .Produces<Response<PaginatedResult<UserProfileCompleteDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/UserProfiles", async (HttpContext context, [FromServices] IUserProfilesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<UserProfileDto>>.Ok(result));
            })
            .WithName("GetUserProfiles")
            .WithDescription("Get all UserProfiles")
            .WithTags("UserProfiles")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<PaginatedResult<UserProfileDto>>>();

            app.MapPost("v1/UserProfile", async (HttpContext context, [FromServices] IUserProfilesService service, [FromBody] UserProfileDto userProfileDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(userProfileDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserProfileDto>.Empty());
                return Results.Ok(Response<UserProfileDto>.Ok(result));
            })
            .WithName("CreateUserProfile")
            .WithDescription("Create an UserProfile")
            .WithTags("UserProfiles")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserProfileDto>>();

            app.MapPut("v1/UserProfile", async (HttpContext context, [FromServices] IUserProfilesService service, [FromBody] UserProfileDto userProfileDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(userProfileDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserProfileDto>.Empty());
                return Results.Ok(Response<UserProfileDto>.Ok(result));
            })
            .WithName("UpdateUserProfile")
            .WithDescription("Update an UserProfile")
            .WithTags("UserProfiles")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserProfileDto>>();

            return app;
        }
    }
}