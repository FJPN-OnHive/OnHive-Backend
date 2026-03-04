using Microsoft.AspNetCore.Mvc;
using OnHive.Authorization.Library.Extensions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Redirects.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using OnHive.Redirects.Services.Extensions;
using OnHive.Core.Library.Enums.Common;
using OnHive.Configuration.Library.Models;

namespace OnHive.Redirects.Api.Endpoints
{
    public static class RedirectEndpoints
    {
        public static WebApplication MapRedirectEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Redirect/{tenantId}/{path}", async (HttpContext context, [FromServices] IRedirectService service, [FromRoute] string tenantId, [FromRoute] string path) =>
            {
                var result = await service.ExecuteRedirect(tenantId, path);
                if (result == null) return Results.NotFound();
                var finalUrl = result.PassParameters ? result.RedirectUrl + context.Request.Query.GetQuery() : result.RedirectUrl;
                switch (result.Type)
                {
                    case Core.Library.Enums.Redirects.RedirectType.Permanent:
                        return Results.Redirect(finalUrl, true, false);

                    case Core.Library.Enums.Redirects.RedirectType.Temporary:
                        return Results.Redirect(finalUrl, false, true);

                    default:
                        return Results.Redirect(finalUrl, false, false);
                }
            })
            .WithName("Redirect")
            .WithDescription("Execute Redirect")
            .WithTags("Redirects")
            .Produces<Response<string>>()
            .AllowAnonymous();

            app.MapGet("v1/Redirect/{RedirectId}", async (HttpContext context, [FromServices] IRedirectService service, [FromRoute] string redirectId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(redirectId);
                if (result == null) return Results.Ok(Response<RedirectDto>.Empty());
                return Results.Ok(Response<RedirectDto>.Ok(result));
            })
            .WithName("GetRedirectById")
            .WithDescription("Get Redirect by Id")
            .WithTags("Redirects")
            .WithMetadata(PermissionConfig.Create("redirect_read"))
            .Produces<Response<RedirectDto>>();

            app.MapGet("v1/Redirects", async (HttpContext context, [FromServices] IRedirectService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<RedirectDto>>.Ok(result));
            })
            .WithName("GetRedirects")
            .WithDescription("Get all Redirects")
            .WithTags("Redirects")
            .WithMetadata(PermissionConfig.Create("redirect_read"))
            .Produces<Response<PaginatedResult<RedirectDto>>>();

            app.MapPost("v1/Redirect", async (HttpContext context, [FromServices] IRedirectService service, [FromBody] RedirectDto redirectDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(redirectDto, loggedUser);
                if (result == null) return Results.Ok(Response<RedirectDto>.Empty());
                return Results.Ok(Response<RedirectDto>.Ok(result));
            })
            .WithName("CreateRedirect")
            .WithDescription("Create an Redirect")
            .WithTags("Redirects")
            .WithMetadata(PermissionConfig.Create("redirect_create"))
            .Produces<Response<RedirectDto>>();

            app.MapPut("v1/Redirect", async (HttpContext context, [FromServices] IRedirectService service, [FromBody] RedirectDto redirectDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(redirectDto, loggedUser);
                if (result == null) return Results.Ok(Response<RedirectDto>.Empty());
                return Results.Ok(Response<RedirectDto>.Ok(result));
            })
            .WithName("UpdateRedirect")
            .WithDescription("Update an Redirect")
            .WithTags("Redirects")
            .WithMetadata(PermissionConfig.Create("redirect_update"))
            .Produces<Response<RedirectDto>>();

            app.MapDelete("v1/Redirect/{RedirectId}", async (HttpContext context, [FromServices] IRedirectService service, [FromRoute] string redirectId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(redirectId, loggedUser);
                if (!result) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(true));
            })
          .WithName("DeleteRedirect")
          .WithDescription("Delete an Redirect")
          .WithTags("Redirects")
          .WithMetadata(PermissionConfig.Create("redirect_update"))
          .Produces<Response<bool>>();

            app.MapGet("v1/Redirects/Export/{tenantId}", async (HttpContext context,
                                                              [FromServices] IRedirectService service,
                                                              [FromRoute] string tenantId,
                                                              [FromQuery] string format = "json",
                                                              [FromQuery] string activeOnly = "true") =>
            {
                if (activeOnly != "true")
                {
                    var loggedUser = context.GetLoggedUser();
                    if (loggedUser?.User == null) return Results.Unauthorized();
                    if (!loggedUser?.User.Permissions.Contains("courses_update") ?? false) return Results.Unauthorized();
                }

                var exportFormat = format.ToLower().Trim() switch
                {
                    "xml" => ExportFormats.Xml,
                    "json" => ExportFormats.Json,
                    _ => ExportFormats.Csv,
                };
                var result = await service.GetExportData(exportFormat, tenantId, activeOnly == "true");
                if (result == null) return Results.NotFound();
                return exportFormat switch
                {
                    ExportFormats.Json => Results.File(result, "text/json", "redirects.json"),
                    ExportFormats.Xml => Results.File(result, "text/xml", "redirects.xml"),
                    _ => Results.File(result, "text/csv", "redirects.csv"),
                };
            })
         .WithName("ExportRedirects")
         .WithDescription("Get Formated Data")
         .WithTags("Redirects")
         .AllowAnonymous()
         .Produces<Stream>();

            return app;
        }
    }
}