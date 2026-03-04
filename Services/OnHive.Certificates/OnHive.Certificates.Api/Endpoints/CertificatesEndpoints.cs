using OnHive.Authorization.Library.Extensions;
using OnHive.Certificates.Domain.Abstractions.Services;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Common;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Certificates.Api.Endpoints
{
    internal static class CertificatesEndpoints
    {
        internal static WebApplication MapCertificatesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Certificate/{CertificateId}", async (HttpContext context, [FromServices] ICertificatesService service, [FromRoute] string CertificateId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(CertificateId);
                if (result == null) return Results.Ok(Response<CertificateDto>.Empty());
                return Results.Ok(Response<CertificateDto>.Ok(result));
            })
            .WithName("GetCertificateById")
            .WithDescription("Get Certificate By Id")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_read"))
            .Produces<Response<CertificateDto>>();

            app.MapGet("v1/Certificates", async (HttpContext context, [FromServices] ICertificatesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<CertificateDto>>.Ok(result));
            })
            .WithName("GetCertificates")
            .WithDescription("Get All Certificates")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_read"))
            .Produces<Response<PaginatedResult<CertificateDto>>>();

            app.MapPost("v1/Certificate", async (HttpContext context, [FromServices] ICertificatesService service, [FromBody] CertificateDto CertificateDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(CertificateDto, loggedUser);
                if (result == null) return Results.Ok(Response<CertificateDto>.Empty());
                return Results.Ok(Response<CertificateDto>.Ok(result));
            })
            .WithName("CreateCertificate")
            .WithDescription("Create a Certificate")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_create"))
            .Produces<Response<CertificateDto>>();

            app.MapPut("v1/Certificate", async (HttpContext context, [FromServices] ICertificatesService service, [FromBody] CertificateDto CertificateDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(CertificateDto, loggedUser);
                if (result == null) return Results.Ok(Response<CertificateDto>.Empty());
                return Results.Ok(Response<CertificateDto>.Ok(result));
            })
            .WithName("UpdateCertificate")
            .WithDescription("Update a Certificate")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_update"))
            .Produces<Response<CertificateDto>>();

            app.MapPatch("v1/Certificate", async (HttpContext context, [FromServices] ICertificatesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<CertificateDto>.Empty());
                return Results.Ok(Response<CertificateDto>.Ok(result));
            })
            .WithName("PatchCertificate")
            .WithDescription("Patch a Certificate")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_update"))
            .Produces<Response<CertificateDto>>();

            app.MapGet("v1/Certificate/Validate/{certificateKey}", async (HttpContext context, [FromServices] ICertificatesService service, [FromRoute] string certificateKey) =>
            {
                var result = await service.ValidateCertificate(certificateKey);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<CertificateMountPublicDto>.Ok(result));
            })
            .WithName("ValidateCertificateByKey")
            .WithDescription("Validate Certificate By Key")
            .WithTags("Certificates")
            .AllowAnonymous()
            .Produces<Response<CertificateMountPublicDto>>();

            app.MapGet("v1/Emmited/Certificate/{CertificateId}", async (HttpContext context, [FromServices] ICertificatesService service, [FromRoute] string certificateId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetEmmitedByIdAsync(certificateId);
                if (result == null) return Results.Ok(Response<CertificateMountDto>.Empty());
                return Results.Ok(Response<CertificateMountDto>.Ok(result));
            })
            .WithName("GetEmmitedCertificateById")
            .WithDescription("Get Emmited Certificate By Id")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_read"))
            .Produces<Response<CertificateMountDto>>();

            app.MapGet("v1/Emmited/Certificates", async (HttpContext context, [FromServices] ICertificatesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetEmmitedByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<CertificateMountDto>>.Ok(result));
            })
            .WithName("GetEmmitedCertificates")
            .WithDescription("Get All Emmited Certificates")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_read"))
            .Produces<Response<PaginatedResult<CertificateMountDto>>>();

            app.MapGet("v1/Emmited/Certificates/ByUser", async (HttpContext context, [FromServices] ICertificatesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetEmmitedByFilterUserAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<CertificateMountDto>>.Ok(result));
            })
            .WithName("GetEmmitedCertificatesByUser")
            .WithDescription("Get All Emmited Certificates ByUser")
            .WithTags("Certificates")
            .WithMetadata(PermissionConfig.Create("certificates_read"))
            .Produces<Response<PaginatedResult<CertificateMountDto>>>();

            return app;
        }
    }
}