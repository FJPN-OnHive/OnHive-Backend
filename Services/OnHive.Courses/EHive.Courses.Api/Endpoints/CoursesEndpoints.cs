using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Enums.Common;
using EHive.Courses.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EHive.Courses.Api.Endpoints
{
    internal static class CoursesEndpoints
    {
        internal static WebApplication MapCoursesEndpoints(this WebApplication app)
        {
            //app.MapGet("v1/Internal/Course/{CourseId}", async (HttpContext context, [FromServices] ICoursesService service, [FromRoute] string courseId) =>
            //{
            //    var result = await service.GetByIdInternalAsync(courseId);
            //    if (result == null) return Results.Ok(Response<CourseDto>.Empty());
            //    return Results.Ok(Response<CourseDto>.Ok(result));
            //})
            //.WithName("InternalGetCourseById")
            //.WithDescription("Internal Get Course By Id")
            //.WithTags("Internal")
            //.Produces<Response<CourseDto>>()
            //.AllowAnonymous();

            //app.MapGet("v1/Internal/Courses/{TenantId}", async (HttpContext context, [FromServices] ICoursesService service, [FromRoute] string tenantId) =>
            //{
            //    var result = await service.GetAllByTenantInternalAsync(tenantId);
            //    if (result == null) return Results.Ok(Response<List<CourseDto>>.Empty());
            //    return Results.Ok(Response<List<CourseDto>>.Ok(result));
            //})
            //.WithName("InternalGetCoursesBtTenant")
            //.WithDescription("Internal Get Course By Tenant Id")
            //.WithTags("Internal")
            //.Produces<Response<List<CourseDto>>>()
            //.AllowAnonymous();

            app.MapGet("v1/Course/{CourseId}", async (HttpContext context, [FromServices] ICoursesService service, [FromRoute] string courseId) =>
            {
                var result = await service.GetByIdAsync(courseId);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<CourseDto>.Ok(result));
            })
            .WithName("GetCourseById")
            .WithDescription("Get Course By Id")
            .WithTags("Courses")
            .Produces<Response<CourseDto>>()
            .AllowAnonymous();

            app.MapGet("v1/Course/Full/{CourseId}", async (HttpContext context, [FromServices] ICoursesService service, [FromRoute] string courseId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(courseId, user);
                if (result == null) return Results.Ok(Response<CourseDto>.Empty());
                return Results.Ok(Response<CourseDto>.Ok(result));
            })
            .WithName("GetCourseFullById")
            .WithDescription("Get Full Course By Id")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<CourseDto>>();

            app.MapGet("v1/Courses", async (HttpContext context, [FromServices] ICoursesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<CourseDto>>.Ok(result));
            })
            .WithName("GetCourses")
            .WithDescription("Get all Courses")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<CourseDto>>>();

            app.MapPost("v1/Courses/ByIds", async (HttpContext context, [FromServices] ICoursesService service, [FromBody] List<string> ids) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByIdsAsync(ids, filter, user);
                return Results.Ok(Response<PaginatedResult<CourseDto>>.Ok(result));
            })
            .WithName("GetCoursesByIds")
            .WithDescription("Get all Courses By Ids")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<CourseDto>>>();

            app.MapGet("v1/Courses/ByUser", async (HttpContext context, [FromServices] ICoursesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterByUserAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<CourseResumeDto>>.Ok(result));
            })
            .WithName("GetCoursesResumeByUser")
            .WithDescription("Get all Courses by user")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<CourseResumeDto>>>();

            app.MapGet("v1/Courses/Resume/{tenantId}", async (HttpContext context, [FromServices] ICoursesService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetResumeByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<CourseResumeDto>>.Ok(result));
            })
            .WithName("GetCoursesResume")
            .WithDescription("Saerch all Courses resume")
            .WithTags("Courses")
            .Produces<Response<PaginatedResult<CourseResumeDto>>>()
            .AllowAnonymous();

            app.MapPost("v1/Course", async (HttpContext context, [FromServices] ICoursesService service, [FromBody] CourseDto courseDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(courseDto, loggedUser);
                if (result == null) return Results.Ok(Response<CourseDto>.Empty());
                return Results.Ok(Response<CourseDto>.Ok(result));
            })
            .WithName("CreateCourse")
            .WithDescription("Create a Course")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_create"))
            .Produces<Response<CourseDto>>();

            app.MapPut("v1/Course", async (HttpContext context, [FromServices] ICoursesService service, [FromBody] CourseDto courseDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(courseDto, loggedUser);
                if (result == null) return Results.Ok(Response<CourseDto>.Empty());
                return Results.Ok(Response<CourseDto>.Ok(result));
            })
            .WithName("UpdateCourse")
            .WithDescription("Update a Course")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<CourseDto>>();

            app.MapPatch("v1/Course", async (HttpContext context, [FromServices] ICoursesService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<CourseDto>.Empty());
                return Results.Ok(Response<CourseDto>.Ok(result));
            })
            .WithName("PatchCourse")
            .WithDescription("Patch a Course")
            .WithTags("Courses")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<CourseDto>>();

            app.MapGet("v1/Courses/Export/{tenantId}", async (HttpContext context,
                                                                [FromServices] ICoursesService service,
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
                    ExportFormats.Json => Results.File(result, "text/json", "courses.json"),
                    ExportFormats.Xml => Results.File(result, "text/xml", "courses.xml"),
                    _ => Results.File(result, "text/csv", "courses.csv"),
                };
            })
           .WithName("ExportCourses")
           .WithDescription("Get Formated Data")
           .WithTags("Courses")
           .AllowAnonymous()
           .Produces<Stream>();

            return app;
        }
    }
}