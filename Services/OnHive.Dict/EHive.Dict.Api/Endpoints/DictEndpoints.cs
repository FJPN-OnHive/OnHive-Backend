using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Dict;
using EHive.WebExtensions.Library;
using EHive.Dict.Domain.Abstractions.Services;
using EHive.Configuration.Library.Models;

namespace EHive.Dict.Api.Endpoints
{
    internal static class DictEndpoints
    {
        internal static WebApplication MapDictEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Value/{ValuesId}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string valuesId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(valuesId);
                if (result == null) return Results.Ok(Response<ValueRegistryDto>.Empty());
                return Results.Ok(Response<ValueRegistryDto>.Ok(result));
            })
            .WithName("GetValueById")
            .WithDescription("Get Values by Id")
            .WithTags("Values")
            .WithMetadata(PermissionConfig.Create("dict_read"))
            .Produces<Response<ValueRegistryDto>>();

            app.MapGet("v1/Value/Complete/{tenantId}/{group}/{key}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string tenantId, [FromRoute] string group, [FromRoute] string key) =>
            {
                var result = await service.GetByGroupAndKeyAsync(tenantId, group, key);
                if (result == null) return Results.Ok(Response<ValueRegistryDto>.Empty());
                return Results.Ok(Response<ValueRegistryDto>.Ok(result));
            })
            .WithName("GetValueByGroupAndKeyComplete")
            .WithDescription("Get Value object by Group And Key")
            .WithTags("Values")
            .Produces<Response<ValueRegistryDto>>()
            .AllowAnonymous();

            app.MapGet("v1/Value/{tenantId}/{group}/{key}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string tenantId, [FromRoute] string group, [FromRoute] string key) =>
            {
                var result = await service.GetByGroupAndKeyAsync(tenantId, group, key);
                if (result == null) return Results.NotFound();
                return Results.Ok(result.Value);
            })
            .WithName("GetValueByGroupAndKey")
            .WithDescription("Get Values by Group And Key")
            .WithTags("Values")
            .Produces<string>()
            .AllowAnonymous();

            app.MapGet("v1/Values", async (HttpContext context, [FromServices] IDictService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<ValueRegistryDto>>.Ok(result));
            })
            .WithName("GetValues")
            .WithDescription("Get all Values")
            .WithTags("Values")
            .WithMetadata(PermissionConfig.Create("dict_read"))
            .Produces<Response<PaginatedResult<ValueRegistryDto>>>();

            app.MapPost("v1/Value", async (HttpContext context, [FromServices] IDictService service, [FromBody] ValueRegistryDto valuesDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(valuesDto, loggedUser);
                if (result == null) return Results.Ok(Response<ValueRegistryDto>.Empty());
                return Results.Ok(Response<ValueRegistryDto>.Ok(result));
            })
            .WithName("CreateValue")
            .WithDescription("Create an Value")
            .WithTags("Values")
            .WithMetadata(PermissionConfig.Create("dict_create"))
            .Produces<Response<ValueRegistryDto>>();

            app.MapPut("v1/Value", async (HttpContext context, [FromServices] IDictService service, [FromBody] ValueRegistryDto valuesDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(valuesDto, loggedUser);
                if (result == null) return Results.Ok(Response<ValueRegistryDto>.Empty());
                return Results.Ok(Response<ValueRegistryDto>.Ok(result));
            })
            .WithName("UpdateValue")
            .WithDescription("Update an Value")
            .WithTags("Values")
            .WithMetadata(PermissionConfig.Create("dict_update"))
            .Produces<Response<ValueRegistryDto>>();

            app.MapDelete("v1/Value/{valueId}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string valueId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(valueId, loggedUser);
                if (!result) return Results.NotFound();
                return Results.Ok(Response<bool>.Ok());
            })
            .WithName("DeleteValue")
            .WithDescription("Delete an Value")
            .WithTags("Values")
            .WithMetadata(PermissionConfig.Create("dict_update"))
            .Produces<Response<bool>>();

            app.MapGet("v1/Value/Groups/{tenantId}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetGroupsAsync(tenantId);
                return Results.Ok(result);
            })
           .WithName("GetGroups")
           .WithDescription("Get Groups")
           .WithTags("Values")
           .Produces<List<string>>()
           .AllowAnonymous();

            app.MapGet("v1/Value/Keys/{tenantId}/{group}", async (HttpContext context, [FromServices] IDictService service, [FromRoute] string tenantId, [FromRoute] string group) =>
            {
                var result = await service.GetKeysAsync(tenantId, group);
                return Results.Ok(result);
            })
           .WithName("GetKeys")
           .WithDescription("Get Keys")
           .WithTags("Values")
           .Produces<List<string>>()
           .AllowAnonymous();

            return app;
        }
    }
}