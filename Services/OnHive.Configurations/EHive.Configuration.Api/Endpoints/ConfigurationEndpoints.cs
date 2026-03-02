using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Domain.Abstractions.Services;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Configuration;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Configuration.Api.Endpoints
{
    internal static class ConfigurationEndpoints
    {
        internal static WebApplication MapConfigurationEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Configuration/{ConfigurationId}", async (HttpContext context, [FromServices] IConfigurationService service, [FromRoute] string ConfigurationId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(ConfigurationId, user);
                if (result == null) return Results.Ok(Response<ConfigItemDto>.Empty());
                return Results.Ok(Response<ConfigItemDto>.Ok(result));
            })
            .WithName("GetConfigurationById")
            .WithDescription("Get Configuration by id")
            .WithTags("Configurations")
            .WithMetadata(PermissionConfig.Create("admin"))
            .Produces<Response<ConfigItemDto>>();

            app.MapGet("v1/Configuration/Key/{ConfigurationKey}", async (HttpContext context, [FromServices] IConfigurationService service, [FromRoute] string ConfigurationKey) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByKeyAsync(ConfigurationKey, user);
                if (result == null) return Results.Ok(Response<ConfigItemDto>.Empty());
                return Results.Ok(Response<ConfigItemDto>.Ok(result));
            })
            .WithName("GetConfigurationByKey")
            .WithDescription("Get Configuration by Key")
            .WithTags("Configurations")
            .WithMetadata(PermissionConfig.Create("admin"))
            .Produces<Response<ConfigItemDto>>();

            app.MapGet("v1/Configurations", async (HttpContext context, [FromServices] IConfigurationService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<ConfigItemDto>>.Ok(result));
            })
            .WithName("GetConfigurations")
            .WithDescription("Get all configurations")
            .WithTags("Configurations")
            .WithMetadata(PermissionConfig.Create("admin"))
            .Produces<Response<PaginatedResult<ConfigItemDto>>>();

            app.MapPost("v1/Configuration", async (HttpContext context, [FromServices] IConfigurationService service, [FromBody] ConfigItemDto ConfigItemDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.SaveAsync(ConfigItemDto, loggedUser);
                if (result == null) return Results.Ok(Response<ConfigItemDto>.Empty());
                return Results.Ok(Response<ConfigItemDto>.Ok(result));
            })
            .WithName("CreateConfiguration")
            .WithDescription("Create a configuration")
            .WithTags("Configurations")
            .WithMetadata(PermissionConfig.Create("admin"))
            .Produces<Response<ConfigItemDto>>();

            return app;
        }
    }
}