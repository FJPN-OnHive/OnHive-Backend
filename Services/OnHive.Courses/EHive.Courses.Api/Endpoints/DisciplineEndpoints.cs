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
    internal static class DisciplineEndpoints
    {
        internal static WebApplication MapDisciplineEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Discipline/{DisciplineId}", async (HttpContext context, [FromServices] IDisciplineService service, [FromRoute] string disciplineId) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(disciplineId, user);
                if (result == null) return Results.Ok(Response<DisciplineDto>.Empty());
                return Results.Ok(Response<DisciplineDto>.Ok(result));
            })
            .WithName("GetDisciplineById")
            .WithDescription("Get Discipline By Id")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<DisciplineDto>>();

            app.MapGet("v1/Disciplines", async (HttpContext context, [FromServices] IDisciplineService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<DisciplineDto>>.Ok(result));
            })
            .WithName("GetDisciplines")
            .WithDescription("Get all Disciplines")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<DisciplineDto>>>();

            app.MapPost("v1/Disciplines/ByIds", async (HttpContext context, [FromServices] IDisciplineService service, [FromBody] List<string> ids) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByIdsAsync(ids, filter, user);
                return Results.Ok(Response<PaginatedResult<DisciplineDto>>.Ok(result));
            })
            .WithName("GetDisciplinesByIds")
            .WithDescription("Get all Disciplines ByIds")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_read"))
            .Produces<Response<PaginatedResult<DisciplineDto>>>();

            app.MapPost("v1/Discipline", async (HttpContext context, [FromServices] IDisciplineService service, [FromBody] DisciplineDto disciplineDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(disciplineDto, loggedUser);
                if (result == null) return Results.Ok(Response<DisciplineDto>.Empty());
                return Results.Ok(Response<DisciplineDto>.Ok(result));
            })
            .WithName("CreateDiscipline")
            .WithDescription("Create a Discipline")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_create"))
            .Produces<Response<DisciplineDto>>();

            app.MapPut("v1/Discipline", async (HttpContext context, [FromServices] IDisciplineService service, [FromBody] DisciplineDto disciplineDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(disciplineDto, loggedUser);
                if (result == null) return Results.Ok(Response<DisciplineDto>.Empty());
                return Results.Ok(Response<DisciplineDto>.Ok(result));
            })
            .WithName("UpdateDiscipline")
            .WithDescription("Update a Discipline")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<DisciplineDto>>();

            app.MapPatch("v1/Discipline", async (HttpContext context, [FromServices] IDisciplineService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<DisciplineDto>.Empty());
                return Results.Ok(Response<DisciplineDto>.Ok(result));
            })
            .WithName("PatchDiscipline")
            .WithDescription("Patch a Discipline")
            .WithTags("Disciplines")
            .WithMetadata(PermissionConfig.Create("courses_update"))
            .Produces<Response<DisciplineDto>>();

            return app;
        }
    }
}