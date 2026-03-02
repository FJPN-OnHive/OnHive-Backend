using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Emails;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Emails.Api.Endpoints
{
    internal static class EmailsEndpoints
    {
        internal static WebApplication MapEmailsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/EmailTemplate/{EmailTemplateId}", async (HttpContext context, [FromServices] IEmailsService service, [FromRoute] string emailTemplateId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(emailTemplateId);
                if (result == null) return Results.Ok(Response<EmailTemplateDto>.Empty());
                return Results.Ok(Response<EmailTemplateDto>.Ok(result));
            })
            .WithName("GetEmailTemplateById")
            .WithDescription("Get email template by id")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_read"))
            .Produces<Response<EmailTemplateDto>>();

            app.MapGet("v1/EmailTemplates", async (HttpContext context, [FromServices] IEmailsService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<EmailTemplateDto>>.Ok(result));
            })
            .WithName("GetEmailTemplates")
            .WithDescription("Get all emails templates")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_read"))
            .Produces<Response<PaginatedResult<EmailTemplateDto>>>();

            app.MapPost("v1/EmailTemplate", async (HttpContext context, [FromServices] IEmailsService service, [FromBody] EmailTemplateDto emailTemplateDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(emailTemplateDto, loggedUser);
                if (result == null) return Results.Ok(Response<EmailTemplateDto>.Empty());
                return Results.Ok(Response<EmailTemplateDto>.Ok(result));
            })
            .WithName("CreateEmailTemplate")
            .WithDescription("Create a email template")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_create"))
            .Produces<Response<EmailTemplateDto>>();

            app.MapPut("v1/EmailTemplate", async (HttpContext context, [FromServices] IEmailsService service, [FromBody] EmailTemplateDto emailTemplateDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(emailTemplateDto, loggedUser);
                if (result == null) return Results.Ok(Response<EmailTemplateDto>.Empty());
                return Results.Ok(Response<EmailTemplateDto>.Ok(result));
            })
            .WithName("UpdateEmailTemplate")
            .WithDescription("Update a email template")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_update"))
            .Produces<Response<EmailTemplateDto>>();

            app.MapPatch("v1/EmailTemplate", async (HttpContext context, [FromServices] IEmailsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.PatchAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<EmailTemplateDto>.Empty());
                return Results.Ok(Response<EmailTemplateDto>.Ok(result));
            })
            .WithName("PatchEmailTemplate")
            .WithDescription("Patch a email template")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_update"))
            .Produces<Response<EmailTemplateDto>>();

            app.MapDelete("v1/EmailTemplate/{EmailTemplateId}", async (HttpContext context, [FromServices] IEmailsService service, [FromRoute] string emailTemplateId) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.DeleteAsync(emailTemplateId, loggedUser);
                if (!result) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("DeleteEmailTemplate")
            .WithDescription("Delete a email template")
            .WithTags("EmailTemplates")
            .WithMetadata(PermissionConfig.Create("emails_update"))
            .Produces<Response<bool>>();

            //app.MapPost("v1/Internal/SendEmail", async (HttpContext context, [FromServices] IEmailsService service, [FromBody] EmailSendDto message) =>
            //{
            //    await service.ComposeEmail(message);
            //    return Results.Ok(Response<bool>.Ok(true));
            //})
            //.WithName("SendEmail")
            //.WithDescription("Send a email by template")
            //.WithTags("Internal")
            //.Produces<Response<bool>>()
            //.AllowAnonymous();

            return app;
        }
    }
}