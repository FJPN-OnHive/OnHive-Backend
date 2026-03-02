using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Posts;
using EHive.Posts.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using System.Text.Json;
using EHive.Posts.Domain.Models;
using EHive.Core.Library.Enums.Common;
using EHive.Configuration.Library.Models;

namespace EHive.Posts.Api.Endpoints
{
    public static class PostsEndpoints
    {
        public static WebApplication MapPostsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Post/{PostId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string postId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(postId);
                if (result == null) return Results.Ok(Response<BlogPostDto>.Empty());
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
            .WithName("GetPostById")
            .WithDescription("Get Post by Id")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_read"))
            .Produces<Response<BlogPostDto>>();

            app.MapGet("v1/Post/ByCourse/{CourseId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByCourseAsync(courseId, loggedUser);
                if (result == null) return Results.Ok(Response<List<BlogPostDto>>.Empty());
                return Results.Ok(Response<List<BlogPostDto>>.Ok(result));
            })
            .WithName("GetPostByCourse")
            .WithDescription("Get Post by CourseId")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_read"))
            .Produces<Response<List<BlogPostDto>>>();

            app.MapGet("v1/Posts", async (HttpContext context, [FromServices] IPostsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<BlogPostDto>>.Ok(result));
            })
            .WithName("GetPosts")
            .WithDescription("Get all Posts")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_read"))
            .Produces<Response<PaginatedResult<BlogPostDto>>>();

            app.MapGet("v1/Posts/Public/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<BlogPostDto>>.Ok(result));
            })
            .WithName("GetPostsByFilter")
            .WithDescription("Get all Posts by filter")
            .WithTags("Posts")
            .Produces<Response<PaginatedResult<BlogPostDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Posts/Canonical/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetCanonicalAsync(tenantId);
                return Results.Ok(Response<Dictionary<string, string>>.Ok(result));
            })
            .WithName("GetCanonicalListByFilter")
            .WithDescription("Get all Canonical urls")
            .WithTags("Posts")
            .Produces<Response<Dictionary<string, string>>>()
            .AllowAnonymous();

            app.MapGet("v1/Posts/Resume/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetResumeByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<BlogPostDto>>.Ok(result));
            })
          .WithName("GetPostsResumeOpen")
          .WithDescription("Get all Posts Resume Open")
          .WithTags("Posts")
          .Produces<Response<PaginatedResult<BlogPostDto>>>()
          .AllowAnonymous();

            app.MapGet("v1/Posts/Resume", async (HttpContext context, [FromServices] IPostsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetResumeByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<BlogPostDto>>.Ok(result));
            })
          .WithName("GetPostsResume")
          .WithDescription("Get all Posts Resume")
          .WithTags("Posts")
          .WithMetadata(PermissionConfig.Create("posts_read"))
          .Produces<Response<PaginatedResult<BlogPostDto>>>();

            app.MapGet("v1/Posts/Public/Resume/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetResumeByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<BlogPostDto>>.Ok(result));
            })
            .WithName("GetPostsResumeByFilter")
            .WithDescription("Get all Posts Resume by filter")
            .WithTags("Posts")
            .Produces<Response<PaginatedResult<BlogPostDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Post/Slug/{tenantId}/{slug}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string slug, [FromRoute] string tenantId) =>
            {
                var result = await service.GetBySlugAsync(slug, tenantId);
                if (result == null) return Results.NotFound();
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
            .WithName("GetPostsBySlug")
            .WithDescription("Get Post by Slug")
            .WithTags("Posts")
            .Produces<Response<BlogPostDto>>()
            .AllowAnonymous();

            app.MapPost("v1/Post", async (HttpContext context, [FromServices] IPostsService service, [FromBody] BlogPostDto blogPostDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(blogPostDto, loggedUser);
                if (result == null) return Results.Ok(Response<BlogPostDto>.Empty());
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
            .WithName("CreatePost")
            .WithDescription("Create an Post")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_create"))
            .Produces<Response<BlogPostDto>>();

            app.MapPost("v1/Post/Draft", async (HttpContext context, [FromServices] IPostsService service, [FromBody] BlogPostDto blogPostDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateDraftAsync(blogPostDto, loggedUser);
                if (result == null) return Results.Ok(Response<BlogPostDto>.Empty());
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
            .WithName("CreatePostDraft")
            .WithDescription("Create an Post Draft")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_create"))
            .Produces<Response<BlogPostDto>>();

            app.MapPut("v1/Post", async (HttpContext context, [FromServices] IPostsService service, [FromBody] BlogPostDto blogPostDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(blogPostDto, loggedUser);
                if (result == null) return Results.Ok(Response<BlogPostDto>.Empty());
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
            .WithName("UpdatePost")
            .WithDescription("Update an Post")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_update"))
            .Produces<Response<BlogPostDto>>();

            app.MapGet("v1/Post/Like/{postId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string postId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.LikeAsync(postId, loggedUser);
                if (result == null) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("LikePostPost")
            .WithDescription("Like Blog Post")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_read"))
            .Produces<Response<bool>>();

            app.MapGet("v1/Post/Unlike/{postId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string postId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UnlikeAsync(postId, loggedUser);
                if (result == null) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("UnlikePostPost")
            .WithDescription("Unlike Blog Post")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_read"))
            .Produces<Response<bool>>();

            app.MapDelete("v1/Post/{PostId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string PostId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.DeleteByIdAsync(PostId, loggedUser);
                if (result == null) return Results.Ok(Response<bool>.Empty());
                return Results.Ok(Response<bool>.Ok(result));
            })
            .WithName("DeletePostById")
            .WithDescription("Delete Post by Id")
            .WithTags("Posts")
            .WithMetadata(PermissionConfig.Create("posts_update"))
            .Produces<Response<bool>>();

            app.MapPatch("v1/Post", async (HttpContext context, [FromServices] IPostsService service, [FromBody] JsonDocument blogPostDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.PatchAsync(blogPostDto, loggedUser);
                if (result == null) return Results.Ok(Response<BlogPostDto>.Empty());
                return Results.Ok(Response<BlogPostDto>.Ok(result));
            })
          .WithName("PatchPost")
          .WithDescription("Patch an Post")
          .WithTags("Posts")
          .WithMetadata(PermissionConfig.Create("posts_update"))
          .Produces<Response<BlogPostDto>>();

            app.MapGet("v1/Posts/FilterScope/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetFilterScopeAsync(tenantId);
                if (result == null) return Results.Ok(Response<FilterScope>.Empty());
                return Results.Ok(Response<FilterScope>.Ok(result));
            })
            .WithName("GetPostFilterScope")
            .WithDescription("Get Filter Scope")
            .WithTags("Posts")
            .Produces<Response<FilterScope>>()
            .AllowAnonymous();

            app.MapGet("v1/Posts/Categories/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetFilterScopeAsync(tenantId);
                if (result == null) return Results.Ok(Response<string>.Empty());
                var categories = result.Fields.Where(f => f.Field.Equals("Categories", StringComparison.InvariantCultureIgnoreCase)).SelectMany(f => f.Values ?? []).ToList();
                return Results.Ok(Response<List<string>>.Ok(categories));
            })
            .WithName("GetPostsCategories")
            .WithDescription("Get Categories")
            .WithTags("Posts")
            .Produces<Response<List<string>>>()
            .AllowAnonymous();

            app.MapGet("v1/Posts/Slugs/{tenantId}", async (HttpContext context, [FromServices] IPostsService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetSlugsAsync(tenantId);
                if (result == null) return Results.Ok(Response<string>.Empty());
                return Results.Ok(Response<List<string>>.Ok(result));
            })
            .WithName("GetPostsSlugs")
            .WithDescription("Get Slugs")
            .WithTags("Posts")
            .Produces<Response<List<string>>>()
            .AllowAnonymous();

            //app.MapGet("v1/Posts/Internal/Schedule", async (HttpContext context, [FromServices] IPostsService service) =>
            //{
            //    var result = await service.ScheduleExecute();
            //    return Results.Ok(result);
            //})
            //.WithName("ExecuteSchedule")
            //.WithDescription("Execute Schedule")
            //.WithTags("Internal")
            //.Produces<string>()
            //.AllowAnonymous();

            //app.MapGet("v1/Posts/Internal/HouseKeeping", async (HttpContext context, [FromServices] IPostsService service) =>
            //{
            //    var result = await service.HouseKeeping();
            //    return Results.Ok(result);
            //})
            //.WithName("ExecuteHouseKeeping")
            //.WithDescription("Execute HouseKeeping")
            //.WithTags("Internal")
            //.Produces<string>()
            //.AllowAnonymous();

            app.MapGet("v1/Posts/Export/{tenantId}", async (HttpContext context,
                                                              [FromServices] IPostsService service,
                                                              [FromRoute] string tenantId,
                                                              [FromQuery] string format = "json",
                                                              [FromQuery] string activeOnly = "true") =>
            {
                if (activeOnly != "true")
                {
                    var loggedUser = context.GetLoggedUser();
                    if (loggedUser?.User == null) return Results.Unauthorized();
                    if (!loggedUser?.User.Permissions.Contains("posts_update") ?? false) return Results.Unauthorized();
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
                    ExportFormats.Json => Results.File(result, "text/json", "posts.json"),
                    ExportFormats.Xml => Results.File(result, "text/xml", "posts.xml"),
                    _ => Results.File(result, "text/csv", "posts.csv"),
                };
            })
            .WithName("ExportPosts")
            .WithDescription("Get Formated Data")
            .WithTags("Posts")
            .AllowAnonymous()
            .Produces<Stream>();

            return app;
        }
    }
}