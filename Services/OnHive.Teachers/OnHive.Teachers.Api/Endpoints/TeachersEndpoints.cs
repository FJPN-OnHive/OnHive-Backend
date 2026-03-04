using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Teachers;
using OnHive.Teachers.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Teachers.Api.Endpoints
{
    internal static class TeachersEndpoints
    {
        internal static WebApplication MapTeachersEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Teacher/{TeacherId}", async (HttpContext context, [FromServices] ITeachersService service, [FromRoute] string teacherId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(teacherId);
                if (result == null) return Results.Ok(Response<TeacherDto>.Empty());
                return Results.Ok(Response<TeacherDto>.Ok(result));
            })
            .WithName("GetTeacherById")
            .WithDescription("Get teacher by ID")
            .WithTags("Teachers")
            .WithMetadata(PermissionConfig.Create("teachers_read"))
            .Produces<Response<TeacherDto>>();

            app.MapGet("v1/Teachers", async (HttpContext context, [FromServices] ITeachersService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<TeacherDto>>.Ok(result));
            })
            .WithName("GetTeachers")
            .WithDescription("Get all teachers")
            .WithTags("Teachers")
            .WithMetadata(PermissionConfig.Create("teachers_read"))
            .Produces<Response<PaginatedResult<TeacherDto>>>();

            app.MapPost("v1/Teacher", async (HttpContext context, [FromServices] ITeachersService service, [FromBody] TeacherDto teacherDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(teacherDto, loggedUser);
                if (result == null) return Results.Ok(Response<TeacherDto>.Empty());
                return Results.Ok(Response<TeacherDto>.Ok(result));
            })
            .WithName("CreateTeacher")
            .WithDescription("Create a teacher")
            .WithTags("Teachers")
            .WithMetadata(PermissionConfig.Create("teachers_create"))
            .Produces<Response<TeacherDto>>();

            app.MapPut("v1/Teacher", async (HttpContext context, [FromServices] ITeachersService service, [FromBody] TeacherDto teacherDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(teacherDto, loggedUser);
                if (result == null) return Results.Ok(Response<TeacherDto>.Empty());
                return Results.Ok(Response<TeacherDto>.Ok(result));
            })
            .WithName("UpdateTeacher")
            .WithDescription("Update a teacher")
            .WithTags("Teachers")
            .WithMetadata(PermissionConfig.Create("teachers_update"))
            .Produces<Response<TeacherDto>>();

            app.MapPatch("v1/Teacher", async (HttpContext context, [FromServices] ITeachersService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<TeacherDto>.Empty());
                return Results.Ok(Response<TeacherDto>.Ok(result));
            })
            .WithName("PatchTeacher")
            .WithDescription("Patch a teacher")
            .WithTags("Teachers")
            .WithMetadata(PermissionConfig.Create("teachers_update"))
            .Produces<Response<TeacherDto>>();

            return app;
        }
    }
}