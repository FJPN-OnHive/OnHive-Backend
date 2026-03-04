using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Students.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.StudentReports.Api.Endpoints
{
    public static class StudentReportsEndpoints
    {
        public static WebApplication MapStudentReportsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/StudentReport/{StudentReportId}", async (HttpContext context, [FromServices] IStudentReportsService service, [FromRoute] string StudentReportId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(StudentReportId, user);
                if (result == null) return Results.Ok(Response<StudentReportDto>.Empty());
                return Results.Ok(Response<StudentReportDto>.Ok(result));
            })
            .WithName("GetStudentReportById")
            .WithDescription("Get StudentReports by ID")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentReportDto>>();

            app.MapGet("v1/StudentReports", async (HttpContext context, [FromServices] IStudentReportsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<StudentReportDto>>.Ok(result));
            })
            .WithName("GetStudentReports")
            .WithDescription("Get all StudentReports")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<PaginatedResult<StudentReportDto>>>();

            app.MapGet("v1/StudentReports/{type}", async (HttpContext context, [FromServices] IStudentReportsService service, [FromRoute] string type) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAndTypeAsync(filter, type, user);
                return Results.Ok(Response<PaginatedResult<StudentReportDto>>.Ok(result));
            })
            .WithName("GetStudentReportsByType")
            .WithDescription("Get all StudentReports By Type")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<PaginatedResult<StudentReportDto>>>();

            app.MapPost("v1/StudentReport", async (HttpContext context, [FromServices] IStudentReportsService service, [FromBody] StudentReportDto StudentReportDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(StudentReportDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentReportDto>.Empty());
                return Results.Ok(Response<StudentReportDto>.Ok(result));
            })
            .WithName("CreateStudentReport")
            .WithDescription("Create a StudentReport")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentReportDto>>();

            app.MapPut("v1/StudentReport", async (HttpContext context, [FromServices] IStudentReportsService service, [FromBody] StudentReportDto StudentReportDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(StudentReportDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentReportDto>.Empty());
                return Results.Ok(Response<StudentReportDto>.Ok(result));
            })
            .WithName("UpdateStudentReport")
            .WithDescription("Update a StudentReport")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentReportDto>>();

            app.MapPatch("v1/StudentReport", async (HttpContext context, [FromServices] IStudentReportsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentReportDto>.Empty());
                return Results.Ok(Response<StudentReportDto>.Ok(result));
            })
            .WithName("PatchStudentReport")
            .WithDescription("Patch a StudentReport")
            .WithTags("StudentReports")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentReportDto>>();

            return app;
        }
    }
}