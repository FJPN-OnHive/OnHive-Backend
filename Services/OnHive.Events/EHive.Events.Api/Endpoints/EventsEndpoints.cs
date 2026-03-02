using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Events.Api.Endpoints
{
    public static class EventsEndpoints
    {
        public static WebApplication MapEventsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/EventRegister/{EventRegisterId}", async (HttpContext context, [FromServices] IEventsService service, [FromRoute] string eventRegisterId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(eventRegisterId);
                if (result == null) return Results.Ok(Response<EventRegisterDto>.Empty());
                return Results.Ok(Response<EventRegisterDto>.Ok(result));
            })
            .WithName("GetEventRegisterById")
            .WithDescription("Get EventRegister by Id")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_read"))
            .Produces<Response<EventRegisterDto>>();

            app.MapGet("v1/EventRegisters", async (HttpContext context, [FromServices] IEventsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<EventRegisterDto>>.Ok(result));
            })
            .WithName("GetEventRegisters")
            .WithDescription("Get all EventRegisters")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_read"))
            .Produces<Response<PaginatedResult<EventRegisterDto>>>();

            app.MapGet("v1/EventRegisters/Resume", async (HttpContext context, [FromServices] IEventsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterResumeAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<EventResumeDto>>.Ok(result));
            })
            .WithName("GetEventRegistersResume")
            .WithDescription("Get all EventRegisters Resume")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_read"))
            .Produces<Response<PaginatedResult<EventResumeDto>>>();

            app.MapPost("v1/EventRegister", async (HttpContext context, [FromServices] IEventsService service, [FromBody] EventRegisterDto eventRegisterDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(eventRegisterDto, loggedUser);
                if (result == null) return Results.Ok(Response<EventRegisterDto>.Empty());
                return Results.Ok(Response<EventRegisterDto>.Ok(result));
            })
            .WithName("CreateEventRegister")
            .WithDescription("Create an EventRegister")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_create"))
            .Produces<Response<EventRegisterDto>>();

            app.MapPut("v1/EventRegister", async (HttpContext context, [FromServices] IEventsService service, [FromBody] EventRegisterDto eventRegisterDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(eventRegisterDto, loggedUser);
                if (result == null) return Results.Ok(Response<EventRegisterDto>.Empty());
                return Results.Ok(Response<EventRegisterDto>.Ok(result));
            })
            .WithName("UpdateEventRegister")
            .WithDescription("Update an EventRegister")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_update"))
            .Produces<Response<EventRegisterDto>>();

            app.MapPost("v1/EventRegister/Register", async (HttpContext context, [FromServices] IEventsService service, [FromBody] EventMessage eventMessage) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                await service.ProcessEvent(eventMessage, false);
                return Results.Ok(Response<EventRegisterDto>.Ok());
            })
            .WithName("EventRegister")
            .WithDescription("Register an Event")
            .WithTags("EventRegisters")
            .WithMetadata(PermissionConfig.Create("events_update"))
            .Produces<Response<string>>();

            //app.MapGet("v1/EventRegister/Internal/HouseKeeping", async (HttpContext context, [FromServices] IEventsService service) =>
            //{
            //    var result = await service.HouseKeepingExecute();
            //    return Results.Ok(result);
            //})
            //.WithName("EventHouseKeeping")
            //.WithDescription("Event HouseKeeping")
            //.WithTags("Internal")
            //.Produces<string>()
            //.AllowAnonymous();

            return app;
        }
    }
}