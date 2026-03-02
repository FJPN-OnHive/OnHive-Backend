using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Videos;
using EHive.Videos.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Videos.Api.Endpoints
{
    internal static class VideosEndpoints
    {
        internal static WebApplication MapVideosEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Video/{VideoId}", async (HttpContext context, [FromServices] IVideosService service, [FromRoute] string videoId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(videoId);
                if (result == null) return Results.Ok(Response<VideoDto>.Empty());
                return Results.Ok(Response<VideoDto>.Ok(result));
            })
            .WithName("GetVideoById")
            .WithDescription("Get Video by Id")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_read"))
            .Produces<Response<VideoDto>>();

            app.MapGet("v1/Videos", async (HttpContext context, [FromServices] IVideosService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<VideoDto>>.Ok(result));
            })
            .WithName("GetVideos")
            .WithDescription("Get all Videos")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_read"))
            .Produces<Response<PaginatedResult<VideoDto>>>();

            app.MapGet("v1/Videos/All", async (HttpContext context, [FromServices] IVideosService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetAllAsync(loggedUser);
                return Results.Ok(Response<IEnumerable<VideoDto>>.Ok(result));
            })
            .WithName("GetAllVideos")
            .WithDescription("Get all Videos, all states")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_admin"))
            .Produces<Response<IEnumerable<VideoDto>>>();

            app.MapPost("v1/Video/UploadLink", async (HttpContext context, [FromServices] IVideosService service, [FromBody] VideoDto videoDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetUploadUrlAsync(videoDto, loggedUser);
                if (result == null) return Results.Ok(Response<VideoUploadDto>.Empty());
                return Results.Ok(Response<VideoUploadDto>.Ok(result));
            })
            .WithName("CreateVideoUploadLink")
            .WithDescription("Create an Video By Upload Link")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_create"))
            .Produces<Response<VideoUploadDto>>();

            app.MapGet("v1/Video/GetLink/{videoId}", async (HttpContext context, [FromServices] IVideosService service, [FromRoute] string videoId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetVideoUrlAsync(videoId, loggedUser);
                if (result == null) return Results.NotFound();
                return Results.Ok(result);
            })
            .WithName("GetVideoLink")
            .WithDescription("Get an Video Link")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_admin"))
            .Produces<string>();

            //app.MapGet("v1/Video/Internal/GetLink/{videoId}", async (HttpContext context, [FromServices] IVideosService service, [FromRoute] string videoId) =>
            //{
            //    var loggedUser = context.GetLoggedUser();
            //    if (loggedUser?.User == null) return Results.Unauthorized();
            //    var result = await service.GetVideoUrlAsync(videoId, loggedUser);
            //    if (result == null) return Results.NotFound();
            //    return Results.Ok(result);
            //})
            //.WithName("GetVideoLinkInternal")
            //.WithDescription("Get an Video Link Internal")
            //.WithTags("Internal")
            //.Produces<string>();

            app.MapPut("v1/Video", async (HttpContext context, [FromServices] IVideosService service, [FromBody] VideoDto videoDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(videoDto, loggedUser);
                if (result == null) return Results.Ok(Response<VideoDto>.Empty());
                return Results.Ok(Response<VideoDto>.Ok(result));
            })
            .WithName("UpdateVideo")
            .WithDescription("Update an Video")
            .WithTags("Videos")
            .WithMetadata(PermissionConfig.Create("videos_update"))
            .Produces<Response<VideoDto>>();

            //  app.MapGet("v1/Video/Internal/CheckProcessing", async (HttpContext context, [FromServices] IVideosService service) =>
            //  {
            //      var result = await service.CheckProcessingVideos();
            //      return Results.Ok(result);
            //  })
            //.WithName("CheckProcessingVideo")
            //.WithDescription("Check Processing Video")
            //.WithTags("Internal")
            //.Produces<string>()
            //.AllowAnonymous();

            return app;
        }
    }
}