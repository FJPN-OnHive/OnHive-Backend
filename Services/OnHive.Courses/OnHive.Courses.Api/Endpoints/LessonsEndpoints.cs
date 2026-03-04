using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Courses.Api.Endpoints
{
    internal static class LessonsEndpoints
    {
        internal static WebApplication MapLessonsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Lesson/{lessonId}", async (HttpContext context, [FromServices] ILessonsService service, [FromRoute] string lessonId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(lessonId, user);
                if (result == null) return Results.Ok(Response<LessonDto>.Empty());
                return Results.Ok(Response<LessonDto>.Ok(result));
            })
            .WithName("GetLessonById")
            .WithDescription("Get Lesson By Id")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<LessonDto>>();

            app.MapPost("v1/Lessons/ByIds", async (HttpContext context, [FromServices] ILessonsService service, [FromBody] List<string> lessonIds) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByIdsAsync(lessonIds, filter, user);
                if (result == null) return Results.Ok(Response<LessonDto>.Empty());
                return Results.Ok(Response<PaginatedResult<LessonDto>>.Ok(result));
            })
            .WithName("GetLessonsByIds")
            .WithDescription("Get Lessons By Ids")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<LessonDto>>>();

            app.MapGet("v1/Lessons", async (HttpContext context, [FromServices] ILessonsService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<LessonDto>>.Ok(result));
            })
            .WithName("GetLessons")
            .WithDescription("Get all Lessons")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<LessonDto>>>();

            app.MapPost("v1/Lesson", async (HttpContext context, [FromServices] ILessonsService service, [FromBody] LessonDto lessonDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(lessonDto, loggedUser);
                if (result == null) return Results.Ok(Response<LessonDto>.Empty());
                return Results.Ok(Response<LessonDto>.Ok(result));
            })
            .WithName("CreateLesson")
            .WithDescription("Create a Lesson")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_create"))
            .Produces<Response<LessonDto>>();

            app.MapPut("v1/Lesson", async (HttpContext context, [FromServices] ILessonsService service, [FromBody] LessonDto lessonDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(lessonDto, loggedUser);
                if (result == null) return Results.Ok(Response<LessonDto>.Empty());
                return Results.Ok(Response<LessonDto>.Ok(result));
            })
            .WithName("UpdateLesson")
            .WithDescription("Update a Lesson")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<LessonDto>>();

            app.MapPatch("v1/Lesson", async (HttpContext context, [FromServices] ILessonsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<LessonDto>.Empty());
                return Results.Ok(Response<LessonDto>.Ok(result));
            })
            .WithName("PatchLesson")
            .WithDescription("Patch a Lesson")
            .WithTags("Lessons")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<LessonDto>>();

            return app;
        }
    }
}