using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.SystemParameters;
using EHive.SystemParameters.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.SystemParameters.Api.Endpoints
{
    internal static class SystemParametersEndpoints
    {
        internal static WebApplication MapSystemParametersEndpoints(this WebApplication app)
        {
            //app.MapGet("v1/Internal/SystemParameter/{SystemParameterId}", async (HttpContext context, [FromServices] ISystemParametersService service, [FromRoute] string systemParameterId) =>
            //{
            //    var result = await service.GetByIdAsync(systemParameterId);
            //    if (result == null) return Results.Ok(Response<SystemParameterDto>.Empty());
            //    return Results.Ok(Response<SystemParameterDto>.Ok(result));
            //})
            //.WithName("GetSystemParameterByIdInternal")
            //.WithDescription("Get system parameters by Id")
            //.WithTags("Internal")
            //.Produces<Response<SystemParameterDto>>()
            //.AllowAnonymous();

            //app.MapGet("v1/Internal/SystemParameter/ByGroup/{group}", async (HttpContext context, [FromServices] ISystemParametersService service, [FromRoute] string group) =>
            //{
            //    var result = await service.GetByGroupAsync(group);
            //    if (result == null) return Results.Ok(Response<IEnumerable<SystemParameterDto>>.Empty());
            //    return Results.Ok(Response<IEnumerable<SystemParameterDto>>.Ok(result));
            //})
            //.WithName("GetSystemParameterByGroupInternal")
            //.WithDescription("Get system parameters by Group")
            //.WithTags("Internal")
            //.Produces<Response<IEnumerable<SystemParameterDto>>>()
            //.AllowAnonymous();

            //app.MapGet("v1/Internal/SystemParameter", async (HttpContext context, [FromServices] ISystemParametersService service) =>
            //{
            //    var result = await service.GetAllAsync();
            //    if (result == null) return Results.Ok(Response<IEnumerable<SystemParameterDto>>.Empty());
            //    return Results.Ok(Response<IEnumerable<SystemParameterDto>>.Ok(result));
            //})
            //.WithName("GetSystemParametersInternal")
            //.WithDescription("Get all system parameters")
            //.WithTags("Internal")
            //.Produces<Response<IEnumerable<SystemParameterDto>>>()
            //.AllowAnonymous();

            app.MapGet("v1/SystemParameter/{SystemParameterId}", async (HttpContext context, [FromServices] ISystemParametersService service, [FromRoute] string systemParameterId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(systemParameterId);
                if (result == null) return Results.Ok(Response<SystemParameterDto>.Empty());
                return Results.Ok(Response<SystemParameterDto>.Ok(result));
            })
            .WithName("GetSystemParameterById")
            .WithDescription("Get system parameters by Id")
            .WithTags("SystemParameters")
            .WithMetadata(PermissionConfig.Create("systemParameters_read"))
            .Produces<Response<SystemParameterDto>>();

            app.MapGet("v1/SystemParameters", async (HttpContext context, [FromServices] ISystemParametersService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<SystemParameterDto>>.Ok(result));
            })
            .WithName("GetSystemParameters")
            .WithDescription("Get all system parameters")
            .WithTags("SystemParameters")
            .WithMetadata(PermissionConfig.Create("systemParameters_read"))
            .Produces<Response<PaginatedResult<SystemParameterDto>>>();

            app.MapPost("v1/SystemParameter", async (HttpContext context, [FromServices] ISystemParametersService service, [FromBody] SystemParameterDto systemParameterDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(systemParameterDto, loggedUser);
                if (result == null) return Results.Ok(Response<SystemParameterDto>.Empty());
                return Results.Ok(Response<SystemParameterDto>.Ok(result));
            })
            .WithName("CreateSystemParameter")
            .WithDescription("Create a system parameter")
            .WithTags("SystemParameters")
            .WithMetadata(PermissionConfig.Create("systemParameters_create"))
            .Produces<Response<SystemParameterDto>>();

            app.MapPut("v1/SystemParameter", async (HttpContext context, [FromServices] ISystemParametersService service, [FromBody] SystemParameterDto systemParameterDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(systemParameterDto, loggedUser);
                if (result == null) return Results.Ok(Response<SystemParameterDto>.Empty());
                return Results.Ok(Response<SystemParameterDto>.Ok(result));
            })
            .WithName("UpdateSystemParameter")
            .WithDescription("Update a system parameter")
            .WithTags("SystemParameters")
            .WithMetadata(PermissionConfig.Create("systemParameters_update"))
            .Produces<Response<SystemParameterDto>>();

            app.MapPatch("v1/SystemParameter", async (HttpContext context, [FromServices] ISystemParametersService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<SystemParameterDto>.Empty());
                return Results.Ok(Response<SystemParameterDto>.Ok(result));
            })
            .WithName("PatchSystemParameter")
            .WithDescription("Patch a system parameter")
            .WithTags("SystemParameters")
            .WithMetadata(PermissionConfig.Create("systemParameters_update"))
            .Produces<Response<SystemParameterDto>>();

            return app;
        }
    }
}