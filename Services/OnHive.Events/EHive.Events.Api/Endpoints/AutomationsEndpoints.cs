using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Extensions;
using EHive.Events.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using EHive.Configuration.Library.Models;

namespace EHive.Events.Api.Endpoints
{
    internal static class AutomationsEndpoints
    {
        internal static WebApplication MapAutomationsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Automation/{AutomationId}", async (HttpContext context, [FromServices] IAutomationsService service, [FromRoute] string automationId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(automationId);
                if (result == null) return Results.Ok(Response<AutomationDto>.Empty());
                return Results.Ok(Response<AutomationDto>.Ok(result));
            })
            .WithName("GetAutomationById")
            .WithDescription("Get Automation by Id")
            .WithTags("Automations")
            .WithMetadata(PermissionConfig.Create("automations_read"))
            .Produces<Response<AutomationDto>>();

            app.MapGet("v1/Automations", async (HttpContext context, [FromServices] IAutomationsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<AutomationDto>>.Ok(result));
            })
            .WithName("GetAutomations")
            .WithDescription("Get all Automations")
            .WithTags("Automations")
            .WithMetadata(PermissionConfig.Create("automations_read"))
            .Produces<Response<PaginatedResult<AutomationDto>>>();

            app.MapPost("v1/Automation", async (HttpContext context, [FromServices] IAutomationsService service, [FromBody] AutomationDto automationDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(automationDto, loggedUser);
                if (result == null) return Results.Ok(Response<AutomationDto>.Empty());
                return Results.Ok(Response<AutomationDto>.Ok(result));
            })
            .WithName("CreateAutomation")
            .WithDescription("Create an Automation")
            .WithTags("Automations")
            .WithMetadata(PermissionConfig.Create("automations_create"))
            .Produces<Response<AutomationDto>>();

            app.MapPut("v1/Automation", async (HttpContext context, [FromServices] IAutomationsService service, [FromBody] AutomationDto automationDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(automationDto, loggedUser);
                if (result == null) return Results.Ok(Response<AutomationDto>.Empty());
                return Results.Ok(Response<AutomationDto>.Ok(result));
            })
            .WithName("UpdateAutomation")
            .WithDescription("Update an Automation")
            .WithTags("Automations")
            .WithMetadata(PermissionConfig.Create("automations_update"))
            .Produces<Response<AutomationDto>>();

            app.MapDelete("v1/Automation/{AutomationId}", async (HttpContext context, [FromServices] IAutomationsService service, [FromRoute] string automationId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(automationId, loggedUser);
                if (!result) return Results.Ok(Response<AutomationDto>.Empty());
                return Results.Ok(Response<AutomationDto>.Ok());
            })
           .WithName("DeleteAutomation")
           .WithDescription("Delete an Automation")
           .WithTags("Automations")
           .WithMetadata(PermissionConfig.Create("automations_update"))
           .Produces<Response<AutomationDto>>();

            return app;
        }
    }
}