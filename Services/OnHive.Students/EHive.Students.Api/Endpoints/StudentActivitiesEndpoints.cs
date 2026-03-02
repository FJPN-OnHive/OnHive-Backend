using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Students;
using EHive.Students.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using System.Text.Json;
using EHive.Configuration.Library.Models;

namespace EHive.Students.Api.Endpoints
{
    internal static class StudentActivitiesEndpoints
    {
        internal static WebApplication MapStudentActivitiesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/StudentActivity/{StudentId}", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromRoute] string studentId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(studentId, user);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<StudentActivityDto>.Ok(result));
            })
            .WithName("GetStudentActivityById")
            .WithDescription("Get students by ID")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentActivityDto>>();

            app.MapGet("v1/StudentActivities/ByCourse/{courseId}", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromRoute] string courseId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByCourseId(user, courseId);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<List<StudentActivityDto>>.Ok(result));
            })
            .WithName("GetStudentActivitiesByCourse")
            .WithDescription("Get student Activities by Course")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<List<StudentActivityDto>>>();

            app.MapGet("v1/StudentActivities/ByCourseAndLesson/{courseId}/{lessonId}", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromRoute] string courseId, [FromRoute] string lessonId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByCourseAndLessonId(user, courseId, lessonId);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<List<StudentActivityDto>>.Ok(result));
            })
            .WithName("GetStudentActivitiesByCourseAndLesson")
            .WithDescription("Get student Activities by Course And Lesson")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<List<StudentActivityDto>>>();

            app.MapGet("v1/StudentActivities/User/ByCourse/{courseId}", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromRoute] string courseId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByCourseIdLoggedUser(user, courseId);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<List<StudentActivityDto>>.Ok(result));
            })
          .WithName("GetStudentActivitiesByCourseLoggedUser")
          .WithDescription("Get student Activities by Course Logged User")
          .WithTags("StudentActivity")
          .WithMetadata(PermissionConfig.Create("students_read"))
          .Produces<Response<List<StudentActivityDto>>>();

            app.MapGet("v1/StudentActivities/User/ByCourseAndLesson/{courseId}/{lessonId}", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromRoute] string courseId, [FromRoute] string lessonId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByCourseAndLessonIdLoggedUser(user, courseId, lessonId);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<List<StudentActivityDto>>.Ok(result));
            })
            .WithName("GetStudentActivitiesByCourseAndLessonLoggedUser")
            .WithDescription("Get student Activities by Course And Lesson Logged User")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<List<StudentActivityDto>>>();

            app.MapGet("v1/StudentActivity", async (HttpContext context, [FromServices] IStudentActivitiesService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByLoggedUserAsync(user);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<StudentActivityDto>.Ok(result));
            })
            .WithName("GetStudentActivitiesByLoggedUser")
            .WithDescription("Get student Activities by Logged User")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<StudentActivityDto>>();

            app.MapGet("v1/StudentActivities", async (HttpContext context, [FromServices] IStudentActivitiesService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<StudentActivityDto>>.Ok(result));
            })
            .WithName("GetStudentActivities")
            .WithDescription("Get all student Activities")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<PaginatedResult<StudentActivityDto>>>();

            app.MapPost("v1/StudentActivity", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromBody] StudentActivityDto StudentActivityDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(StudentActivityDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<StudentActivityDto>.Ok(result));
            })
            .WithName("CreateStudentActivity")
            .WithDescription("Create a student Activity")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentActivityDto>>();

            app.MapPut("v1/StudentActivity", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromBody] StudentActivityDto StudentActivityDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(StudentActivityDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<StudentActivityDto>.Ok(result));
            })
            .WithName("UpdateStudentActivity")
            .WithDescription("Update a student Activity")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentActivityDto>>();

            app.MapPatch("v1/StudentActivity", async (HttpContext context, [FromServices] IStudentActivitiesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentActivityDto>.Empty());
                return Results.Ok(Response<StudentActivityDto>.Ok(result));
            })
            .WithName("PatchStudentActivity")
            .WithDescription("Patch a student Activity")
            .WithTags("StudentActivity")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentActivityDto>>();

            return app;
        }
    }
}