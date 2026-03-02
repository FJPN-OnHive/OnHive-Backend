using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Courses.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Courses.Api.Endpoints
{
    internal static class ExamsEndpoints
    {
        internal static WebApplication MapExamsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Exam/{ExamId}", async (HttpContext context, [FromServices] IExamsService service, [FromRoute] string examId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(examId, user);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("GetExamById")
            .WithDescription("Get a Exam By Id")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<ExamDto>>();

            app.MapGet("v1/Exams", async (HttpContext context, [FromServices] IExamsService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<ExamDto>>.Ok(result));
            })
            .WithName("GetExams")
            .WithDescription("Get all Exams")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<ExamDto>>>();

            app.MapPost("v1/Exam", async (HttpContext context, [FromServices] IExamsService service, [FromBody] ExamDto examDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(examDto, loggedUser);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("CreateExam")
            .WithDescription("Create a Exam")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_create"))
            .Produces<Response<ExamDto>>();

            app.MapPost("v1/Exam/NewVersion", async (HttpContext context, [FromServices] IExamsService service, [FromBody] ExamDto examDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateVersionAsync(examDto, loggedUser);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("CreateExamVersion")
            .WithDescription("Create a Exam Version")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_create"))
            .Produces<Response<ExamDto>>();

            app.MapPut("v1/Exam", async (HttpContext context, [FromServices] IExamsService service, [FromBody] ExamDto examDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(examDto, loggedUser);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("UpdateExam")
            .WithDescription("Update a Exam")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<ExamDto>>();

            app.MapPatch("v1/Exam", async (HttpContext context, [FromServices] IExamsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("PatchExam")
            .WithDescription("Patch a Exam")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<ExamDto>>();

            app.MapGet("v1/Exam/Versions/{vId}", async (HttpContext context, [FromServices] IExamsService service, [FromRoute] string vId) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetVersionsAsync(vId, loggedUser);
                if (result == null) return Results.Ok(Response<List<ExamDto>>.Empty());
                return Results.Ok(Response<List<ExamDto>>.Ok(result));
            })
           .WithName("ListExamVersions")
           .WithDescription("Get exam versions")
           .WithTags("Exams")
           .WithMetadata(PermissionConfig.Create("courses_read"))
           .Produces<Response<List<ExamDto>>>();

            app.MapGet("v1/Exam/Version/{ExamId}/{versionNumber}", async (HttpContext context, [FromServices] IExamsService service, [FromRoute] string examId, [FromRoute] int versionNumber) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetVersionAsync(examId, versionNumber, user);
                if (result == null) return Results.Ok(Response<ExamDto>.Empty());
                return Results.Ok(Response<ExamDto>.Ok(result));
            })
            .WithName("GetExamVersionById")
            .WithDescription("Get a Exam By Id")
            .WithTags("Exams")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<ExamDto>>();

            //app.MapGet("v1/Internal/Exam/Version/{ExamId}/{versionNumber}", async (HttpContext context, [FromServices] IExamsService service, [FromRoute] string examId, [FromRoute] int versionNumber) =>
            //{
            //    var result = await service.GetVersionInternalAsync(examId, versionNumber);
            //    if (result == null) return Results.Ok(Response<ExamDto>.Empty());
            //    return Results.Ok(Response<ExamDto>.Ok(result));
            //})
            //.WithName("GetExamVersionInternalById")
            //.WithDescription("Get a Exam Version Interna By Id")
            //.WithTags("Internal")
            //.Produces<Response<ExamDto>>()
            //.AllowAnonymous();

            return app;
        }
    }
}