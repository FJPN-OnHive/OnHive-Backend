using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Events.Api.Endpoints
{
    public static class WebHooksEndpoints
    {
        public static WebApplication MapWebHooksEndpoints(this WebApplication app)
        {
            app.MapGet("v1/WebHook/Receive/{tenantId}/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string tenantId, [FromRoute] string slug) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(tenantId, slug, "GET", null, headers, query, false);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookGET")
            .WithDescription("Receive WebHook GET")
            .WithTags("WebHooks")
            .AllowAnonymous();

            app.MapPost("v1/WebHook/Receive/{tenantId}/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string tenantId, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(tenantId, slug, "POST", body, headers, query, false);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPOST")
            .WithDescription("Receive WebHook POST")
            .WithTags("WebHooks")
            .AllowAnonymous();

            app.MapPut("v1/WebHook/Receive/{tenantId}/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string tenantId, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(tenantId, slug, "PUT", body, headers, query, false);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPUT")
            .WithDescription("Receive WebHook PUT")
            .WithTags("WebHooks")
            .AllowAnonymous();

            app.MapPatch("v1/WebHook/Receive/{tenantId}/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string tenantId, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(tenantId, slug, "PATCH", body, headers, query, false);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPATCH")
            .WithDescription("Receive WebHook PATCH")
            .WithTags("WebHooks")
            .AllowAnonymous();

            app.MapGet("v1/WebHook/Receive/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string slug) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(loggedUser.User.TenantId, slug, "GET", null, headers, query, true);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookGETauth")
            .WithDescription("Receive WebHook GET auth")
            .WithMetadata(PermissionConfig.Create("webhooks_receive"))
            .WithTags("WebHooks");

            app.MapPost("v1/WebHook/Receive/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(loggedUser.User.TenantId, slug, "POST", body, headers, query, true);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPOSTauth")
            .WithDescription("Receive WebHook POST auth")
            .WithMetadata(PermissionConfig.Create("webhooks_receive"))
            .WithTags("WebHooks");

            app.MapPut("v1/WebHook/Receive/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(loggedUser.User.TenantId, slug, "PUT", body, headers, query, true);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPUTauth")
            .WithDescription("Receive WebHook PUT auth")
            .WithMetadata(PermissionConfig.Create("webhooks_receive"))
            .WithTags("WebHooks");

            app.MapPatch("v1/WebHook/Receive/{slug}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string slug, [FromBody] JsonDocument body) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.Receive(loggedUser.User.TenantId, slug, "PATCH", body, headers, query, true);
                return Results.Ok();
            })
            .WithName("ReceiveWebHookPATCHauth")
            .WithDescription("Receive WebHook PATCH auth")
            .WithMetadata(PermissionConfig.Create("webhooks_receive"))
            .WithTags("WebHooks");

            app.MapGet("v1/WebHook/{WebHookId}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string webHookId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(webHookId);
                if (result == null) return Results.Ok(Response<WebHookDto>.Empty());
                return Results.Ok(Response<WebHookDto>.Ok(result));
            })
            .WithName("GetWebHookById")
            .WithDescription("Get WebHook by Id")
            .WithTags("WebHooks")
            .WithMetadata(PermissionConfig.Create("webhooks_read"))
            .Produces<Response<WebHookDto>>();

            app.MapGet("v1/WebHooks", async (HttpContext context, [FromServices] IWebHooksService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<WebHookDto>>.Ok(result));
            })
            .WithName("GetWebHooks")
            .WithDescription("Get all WebHooks")
            .WithTags("WebHooks")
            .WithMetadata(PermissionConfig.Create("webhooks_read"))
            .Produces<Response<PaginatedResult<WebHookDto>>>();

            app.MapPost("v1/WebHook", async (HttpContext context, [FromServices] IWebHooksService service, [FromBody] WebHookDto webHookDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(webHookDto, loggedUser);
                if (result == null) return Results.Ok(Response<WebHookDto>.Empty());
                return Results.Ok(Response<WebHookDto>.Ok(result));
            })
            .WithName("CreateWebHook")
            .WithDescription("Create an WebHook")
            .WithTags("WebHooks")
            .WithMetadata(PermissionConfig.Create("webhooks_create"))
            .Produces<Response<WebHookDto>>();

            app.MapPut("v1/WebHook", async (HttpContext context, [FromServices] IWebHooksService service, [FromBody] WebHookDto webHookDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(webHookDto, loggedUser);
                if (result == null) return Results.Ok(Response<WebHookDto>.Empty());
                return Results.Ok(Response<WebHookDto>.Ok(result));
            })
            .WithName("UpdateWebHook")
            .WithDescription("Update an WebHook")
            .WithTags("WebHooks")
            .WithMetadata(PermissionConfig.Create("webhooks_update"))
            .Produces<Response<WebHookDto>>();

            app.MapDelete("v1/WebHook/{WebHookId}", async (HttpContext context, [FromServices] IWebHooksService service, [FromRoute] string webHookId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                await service.DeleteById(webHookId, loggedUser);
                return Results.Ok(Response<WebHookDto>.Ok("Deleted"));
            })
          .WithName("DeleteWebHookById")
          .WithDescription("Delete WebHook by Id")
          .WithTags("WebHooks")
          .WithMetadata(PermissionConfig.Create("webhooks_delete"))
          .Produces<Response<string>>();

            return app;
        }
    }
}