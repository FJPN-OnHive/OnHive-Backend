using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Storages;
using EHive.Storages.Domain.Abstractions.Services;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Storages.Api.Endpoints
{
    internal static class StorageImagesEndpoints
    {
        internal static WebApplication MapStorageImagesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Storage/Image/{StorageId}", async (HttpContext context, [FromServices] IStorageImagesService service, [FromRoute] string storageId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(storageId);
                if (result == null) return Results.Ok(Response<StorageImageFileDto>.Empty());
                return Results.Ok(Response<StorageImageFileDto>.Ok(result));
            })
            .WithName("GetImageById")
            .WithDescription("Get Storage by Id")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Response<StorageImageFileDto>>();

            app.MapGet("v1/Storage/Images", async (HttpContext context, [FromServices] IStorageImagesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<StorageImageFileDto>>.Ok(result));
            })
            .WithName("GetImages")
            .WithDescription("Get all Storages")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Response<PaginatedResult<StorageImageFileDto>>>();

            app.MapGet("v1/Storage/Images/{tenantId}", async (HttpContext context, [FromServices] IStorageImagesService service, string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByFilterOpenAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<StorageImageFileDto>>.Ok(result));
            })
            .WithName("GetImagesPublic")
            .WithDescription("Get all Public Storages Images")
            .WithTags("Storages")
            .Produces<Response<PaginatedResult<StorageImageFileDto>>>()
            .AllowAnonymous();

            app.MapPut("v1/Storage/Image", async (HttpContext context, [FromServices] IStorageImagesService service, [FromBody] StorageImageFileDto storageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(storageDto, loggedUser);
                if (result == null) return Results.Ok(Response<StorageImageFileDto>.Empty());
                return Results.Ok(Response<StorageImageFileDto>.Ok(result));
            })
            .WithName("UpdateImage")
            .WithDescription("Update an Storage")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_update"))
            .Produces<Response<StorageImageFileDto>>();

            app.MapPost("v1/Storage/Image", async (HttpContext context, [FromServices] IStorageImagesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var file = context.Request.Form.Files.FirstOrDefault();
                if (file == null) return Results.BadRequest("File not found");
                context.Request.Form.TryGetValue("imageId", out var imageId);
                context.Request.Form.TryGetValue("name", out var name);
                context.Request.Form.TryGetValue("altText", out var altText);
                context.Request.Form.TryGetValue("subtitle", out var subtitle);
                context.Request.Form.TryGetValue("tags", out var tags);
                context.Request.Form.TryGetValue("categories", out var categories);
                context.Request.Form.TryGetValue("public", out var publicImage);
                context.Request.Form.TryGetValue("noConvert", out var noConvertString);
                var noConvert = !string.IsNullOrEmpty(noConvertString.ToString()) && bool.Parse(noConvertString.ToString());

                var storageDto = new StorageImageFileDto
                {
                    ImageId = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}",
                    Name = name.ToString() ?? file.FileName,
                    AltText = altText.ToString() ?? string.Empty,
                    Subtitle = subtitle.ToString() ?? string.Empty,
                    OriginalFileName = file.FileName,
                    Public = string.IsNullOrEmpty(publicImage.ToString()) || bool.Parse(publicImage.ToString()),
                    Tags = tags.Count > 0 ? tags.ToString().Split(',').ToList() : [],
                    Categories = categories.Count > 0 ? categories.ToString().Split(',').ToList() : []
                };
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var result = await service.UploadImageAsync(memoryStream, storageDto, noConvert, loggedUser);
                if (result == null) return Results.Ok(Response<StorageImageFileDto>.Empty());
                return Results.Ok(Response<StorageImageFileDto>.Ok(result));
            })
            .WithName("UploadImage")
            .WithDescription("Upload Image")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_create"))
            .Produces<Response<StorageImageFileDto>>();

            app.MapGet("v1/Storage/Image/{tenantId}/{imageId}", async (HttpContext context, [FromServices] IStorageImagesService service, [FromRoute] string tenantId, [FromRoute] string imageId, [FromQuery] string res = "hires") =>
            {
                var result = await service.GetImageAsync(imageId, res, tenantId);
                if (imageId.EndsWith(".svg"))
                {
                    return Results.File(result, "image/svg+xml");
                }
                else if (imageId.EndsWith(".gif"))
                {
                    return Results.File(result, "image/gif");
                }
                else if (imageId.EndsWith(".ico"))
                {
                    return Results.File(result, "image/x-icon");
                }
                else if (imageId.EndsWith(".webp"))
                {
                    return Results.File(result, "image/webp");
                }
                return Results.File(result, $"image/{Path.GetExtension(imageId).Replace(".", "")}");
            })
            .WithName("GetImage")
            .WithDescription("Get Image")
            .WithTags("Storages")
            .Produces<Stream>()
            .AllowAnonymous();

            app.MapGet("v1/Storage/PrivateImage/{imageId}", async (HttpContext context, [FromServices] IStorageImagesService service, [FromRoute] string imageId, [FromQuery] string res = "hires") =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetImageAsync(imageId, res, loggedUser);
                if (imageId.EndsWith(".svg"))
                {
                    return Results.File(result, "image/svg+xml");
                }
                else if (imageId.EndsWith(".gif"))
                {
                    return Results.File(result, "image/gif");
                }
                else if (imageId.EndsWith(".ico"))
                {
                    return Results.File(result, "image/x-icon");
                }
                else if (imageId.EndsWith(".webp"))
                {
                    return Results.File(result, "image/webp");
                }
                return Results.File(result, $"image/{Path.GetExtension(imageId).Replace(".", "")}");
            })
            .WithName("GetPrivateImage")
            .WithDescription("Get Private Image")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Stream>();

            app.MapDelete("v1/Storage/Image/{imageId}", async (HttpContext context, [FromServices] IStorageImagesService service, [FromRoute] string imageId) =>
             {
                 var loggedUser = context.GetLoggedUser();
                 if (loggedUser?.User == null) return Results.Unauthorized();
                 await service.DeleteImageAsync(imageId, loggedUser);
                 return Results.Ok(Response<string>.Ok());
             })
             .WithName("DeleteImage")
             .WithDescription("Delete Image")
             .WithTags("Storages")
             .WithMetadata(PermissionConfig.Create("storages_update"))
             .Produces<Response<string>>();

            return app;
        }
    }
}