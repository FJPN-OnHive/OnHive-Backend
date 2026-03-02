using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Events.Api.Endpoints
{
    public static class IntegrationsEndpoints
    {
        public static WebApplication MapIntegrationsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Integrations/{tenantId}/Mautic", async (HttpContext context, [FromServices] IIntegrationsService service, [FromRoute] string tenantId) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.MauticAPI(tenantId, headers, query);
                return Results.Ok();
            })
            .WithName("IntegrationMauticGet")
            .WithDescription("Integration Mautic Get")
            .WithTags("Integrations")
            .AllowAnonymous();

            app.MapPost("v1/Integrations/{tenantId}/Mautic", async (HttpContext context, [FromServices] IIntegrationsService service, [FromRoute] string tenantId, [FromBody] JsonDocument body) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                await service.MauticAPI(tenantId, body, headers, query);
                return Results.Ok();
            })
            .WithName("IntegrationMauticPost")
            .WithDescription("Integration Mautic Post")
            .WithTags("Integrations")
            .AllowAnonymous();

            app.MapPost("v1/Integrations/{tenantId}/Mautic/Form", async (HttpContext context, [FromServices] IIntegrationsService service, [FromRoute] string tenantId) =>
            {
                var headers = context.Request.Headers.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var query = context.Request.Query.ToDictionary().Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToDictionary();
                var formData = context.Request.Form.ToDictionary();
                await service.MauticAPI(tenantId, formData, headers, query);
                return Results.Ok();
            })
            .WithName("IntegrationMauticPostForm")
            .WithDescription("Integration Mautic Post Form Data")
            .WithTags("Integrations")
            .AllowAnonymous();

            app.MapGet("v1/Integrations/MauticSettings", async (HttpContext context, [FromServices] IIntegrationsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetMauticSettings(loggedUser);
                if (result == null) return Results.Ok(Response<MauticIntegrationDto>.Empty());
                return Results.Ok(Response<MauticIntegrationDto>.Ok(result));
            })
            .WithName("GetMauticIntegrationsSettings")
            .WithDescription("Get Mautic Integrations Settings")
            .WithTags("Integrations")
            .WithMetadata(PermissionConfig.Create("integrations_read"))
            .Produces<Response<MauticIntegrationDto>>();

            app.MapPut("v1/Integrations/MauticSettings", async (HttpContext context, [FromServices] IIntegrationsService service, [FromBody] MauticIntegrationDto integration) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateMauticSettings(integration, loggedUser);
                if (result == null) return Results.Ok(Response<MauticIntegrationDto>.Empty());
                return Results.Ok(Response<MauticIntegrationDto>.Ok(result));
            })
            .WithName("UpdateMauticIntegrationsSettings")
            .WithDescription("Update Mautic Integrations Settings")
            .WithTags("Integrations")
            .WithMetadata(PermissionConfig.Create("integrations_update"))
            .Produces<Response<MauticIntegrationDto>>();

            app.MapPost("v1/Integrations/MauticSettings", async (HttpContext context, [FromServices] IIntegrationsService service, [FromBody] MauticIntegrationDto integration) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateMauticSettings(integration, loggedUser);
                if (result == null) return Results.Ok(Response<MauticIntegrationDto>.Empty());
                return Results.Ok(Response<MauticIntegrationDto>.Ok(result));
            })
            .WithName("CreateMauticIntegrationsSettings")
            .WithDescription("Create Mautic Integrations Settings")
            .WithTags("Integrations")
            .WithMetadata(PermissionConfig.Create("integrations_create"))
            .Produces<Response<MauticIntegrationDto>>();

            return app;
        }
    }
}