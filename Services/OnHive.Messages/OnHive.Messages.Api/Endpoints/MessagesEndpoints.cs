using Microsoft.AspNetCore.Mvc;
using OnHive.Authorization.Library.Extensions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.WebExtensions.Library;
using OnHive.Messages.Domain.Abstractions.Services;
using System.Text.Json;

namespace OnHive.Messages.Api.Endpoints
{
    public static class MessagesEndpoints
    {
        public static WebApplication MapMessagesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Messages/User", async (HttpContext context, [FromServices] IMessagesService service, [FromQuery(Name = "newOnly")] bool newOnly = true) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetListByUserAsync(loggedUser, newOnly);
                if (result == null) return Results.Ok(Response<List<MessageUserDto>>.Empty());
                return Results.Ok(Response<List<MessageUserDto>>.Ok(result));
            })
            .WithName("GetMessageListByUser")
            .WithDescription("Get Message List by User")
            .WithTags("Messages")
            .Produces<Response<List<MessageUserDto>>>()
            .WithOpenApi();

            app.MapGet("v1/Message/{MessageId}", async (HttpContext context, [FromServices] IMessagesService service, [FromRoute] string messageId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(messageId, loggedUser);
                if (result == null) return Results.Ok(Response<MessageDto>.Empty());
                return Results.Ok(Response<MessageDto>.Ok(result));
            })
            .WithName("GetMessageById")
            .WithDescription("Get Message by Id")
            .WithTags("Messages")
            .Produces<Response<MessageDto>>()
            .WithOpenApi();

            app.MapGet("v1/Messages", async (HttpContext context, [FromServices] IMessagesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<MessageDto>>.Ok(result));
            })
            .WithName("GetMessages")
            .WithDescription("Get all Messages")
            .WithTags("Messages")
            .Produces<Response<PaginatedResult<MessageDto>>>()
            .WithOpenApi();

            app.MapPost("v1/Message", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] MessageDto messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageDto>.Empty());
                return Results.Ok(Response<MessageDto>.Ok(result));
            })
            .WithName("CreateMessage")
            .WithDescription("Create an Message")
            .WithTags("Messages")
            .Produces<Response<MessageDto>>()
            .WithOpenApi();

            app.MapPut("v1/Message", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] MessageDto messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageDto>.Empty());
                return Results.Ok(Response<MessageDto>.Ok(result));
            })
            .WithName("UpdateMessage")
            .WithDescription("Update an Message")
            .WithTags("Messages")
            .Produces<Response<MessageDto>>()
            .WithOpenApi();

            app.MapPut("v1/Message/User", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] MessageUserDto messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateUserMessageAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageUserDto>.Empty());
                return Results.Ok(Response<MessageUserDto>.Ok(result));
            })
            .WithName("UpdateUserMessage")
            .WithDescription("Update an User Message")
            .WithTags("Messages")
            .Produces<Response<MessageUserDto>>()
            .WithOpenApi();

            app.MapPatch("v1/Message/User", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] JsonDocument messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.PatchUserMessageAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageUserDto>.Empty());
                return Results.Ok(Response<MessageUserDto>.Ok(result));
            })
           .WithName("PatchUserMessage")
           .WithDescription("Patch an User Message")
           .WithTags("Messages")
           .Produces<Response<MessageUserDto>>()
           .WithOpenApi();

            app.MapPatch("v1/Message", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] JsonDocument messageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.PatchAsync(messageDto, loggedUser);
                if (result == null) return Results.Ok(Response<MessageDto>.Empty());
                return Results.Ok(Response<MessageDto>.Ok(result));
            })
          .WithName("PatchMessage")
          .WithDescription("Patch an Message")
          .WithTags("Messages")
          .Produces<Response<MessageDto>>()
          .WithOpenApi();

            app.MapPost("v1/Message/Send", async (HttpContext context, [FromServices] IMessagesService service, [FromBody] MessageDto messageDto) =>
            {
                await service.SendMessageAsync(messageDto);
                return Results.Ok(Response<string>.Ok("Ok"));
            })
            .WithName("SendMessage")
            .WithDescription("Send a Message")
            .WithTags("Messages")
            .AllowAnonymous()
            .Produces<Response<string>>()
            .WithOpenApi();

            return app;
        }
    }
}