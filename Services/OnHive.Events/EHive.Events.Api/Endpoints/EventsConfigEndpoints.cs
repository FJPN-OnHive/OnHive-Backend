using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Events.Api.Endpoints
{
    public static class EventsConfigEndpoints
    {
        public static WebApplication MapEventsConfigEndpoints(this WebApplication app)
        {
            app.MapGet("v1/EventConfig/{EventConfigId}", async (HttpContext context, [FromServices] IEventsConfigService service, [FromRoute] string eventConfigId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(eventConfigId);
                if (result == null) return Results.Ok(Response<EventConfigDto>.Empty());
                return Results.Ok(Response<EventConfigDto>.Ok(result));
            })
            .WithName("GetEventConfigById")
            .WithDescription("Get EventConfig by Id")
            .WithTags("EventConfigs")
            .WithMetadata(PermissionConfig.Create("events_read"))
            .Produces<Response<EventConfigDto>>();

            app.MapGet("v1/EventConfigs", async (HttpContext context, [FromServices] IEventsConfigService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<EventConfigDto>>.Ok(result));
            })
            .WithName("GetEventConfigs")
            .WithDescription("Get all EventConfigs")
            .WithTags("EventConfigs")
            .WithMetadata(PermissionConfig.Create("events_read"))
            .Produces<Response<PaginatedResult<EventConfigDto>>>();

            app.MapPost("v1/EventConfig", async (HttpContext context, [FromServices] IEventsConfigService service, [FromBody] EventConfigDto eventConfigDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(eventConfigDto, loggedUser);
                if (result == null) return Results.Ok(Response<EventConfigDto>.Empty());
                return Results.Ok(Response<EventConfigDto>.Ok(result));
            })
            .WithName("CreateEventConfig")
            .WithDescription("Create an EventConfig")
            .WithTags("EventConfigs")
            .WithMetadata(PermissionConfig.Create("events_create"))
            .Produces<Response<EventConfigDto>>();

            app.MapPut("v1/EventConfig", async (HttpContext context, [FromServices] IEventsConfigService service, [FromBody] EventConfigDto eventConfigDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(eventConfigDto, loggedUser);
                if (result == null) return Results.Ok(Response<EventConfigDto>.Empty());
                return Results.Ok(Response<EventConfigDto>.Ok(result));
            })
            .WithName("UpdateEventConfig")
            .WithDescription("Update an EventConfig")
            .WithTags("EventConfigs")
            .WithMetadata(PermissionConfig.Create("events_update"))
            .Produces<Response<EventConfigDto>>();

            return app;
        }
    }
}