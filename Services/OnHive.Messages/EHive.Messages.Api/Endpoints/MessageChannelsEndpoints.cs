using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Messages;
using EHive.WebExtensions.Library;
using EHive.Messages.Domain.Abstractions.Services;

namespace EHive.Messages.Api.Endpoints
{
    public static class MessageChannelsEndpoints
    {
        public static WebApplication MapMessageChannelsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/MessageChannel/{MessageId}", async (HttpContext context, [FromServices] IMessageChannelsService service, [FromRoute] string messageId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(messageId);
                if (result == null) return Results.Ok(Response<MessageChannelDto>.Empty());
                return Results.Ok(Response<MessageChannelDto>.Ok(result));
            })
            .WithName("GetMessageChannelById")
            .WithDescription("Get Message Channel by Id")
            .WithTags("Messages")
            .Produces<Response<MessageChannelDto>>()
            .WithOpenApi();

            app.MapGet("v1/MessageChannels", async (HttpContext context, [FromServices] IMessageChannelsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<MessageChannelDto>>.Ok(result));
            })
            .WithName("GetMessageChannels")
            .WithDescription("Get all Message Channels")
            .WithTags("Messages")
            .Produces<Response<PaginatedResult<MessageChannelDto>>>()
            .WithOpenApi();

            app.MapGet("v1/MessageChannels/{tenantId}", async (HttpContext context, [FromServices] IMessageChannelsService service, string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByTenant(filter, tenantId);
                return Results.Ok(Response<List<string>>.Ok(result));
            })
            .WithName("GetMessageChannelsByTenant")
            .WithDescription("Get all Message Channels by Tenant")
            .WithTags("Messages")
            .Produces<Response<List<string>>>()
            .AllowAnonymous()
            .WithOpenApi();

            app.MapPost("v1/MessageChannel", async (HttpContext context, [FromServices] IMessageChannelsService service, [FromBody] MessageChannelDto messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageChannelDto>.Empty());
                return Results.Ok(Response<MessageChannelDto>.Ok(result));
            })
            .WithName("CreateMessageChannel")
            .WithDescription("Create an Message Channel")
            .WithTags("Messages")
            .Produces<Response<MessageChannelDto>>()
            .WithOpenApi();

            app.MapPut("v1/MessageChannel", async (HttpContext context, [FromServices] IMessageChannelsService service, [FromBody] MessageChannelDto messageChannelDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(messageChannelDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageChannelDto>.Empty());
                return Results.Ok(Response<MessageChannelDto>.Ok(result));
            })
            .WithName("UpdateMessageChannel")
            .WithDescription("Update an Message Channel")
            .WithTags("Messages")
            .Produces<Response<MessageChannelDto>>()
            .WithOpenApi();

            app.MapDelete("v1/MessageChannel/{MessageId}", async (HttpContext context, [FromServices] IMessageChannelsService service, [FromRoute] string messageId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(messageId, loggedUser);
                if (!result) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("DeleteMessageChannelById")
            .WithDescription("Delete Message Channel by Id")
            .WithTags("Messages")
            .Produces<Response<bool>>()
            .WithOpenApi();

            return app;
        }
    }
}