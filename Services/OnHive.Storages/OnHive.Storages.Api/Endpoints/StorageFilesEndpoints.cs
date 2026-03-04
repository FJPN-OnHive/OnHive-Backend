using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Storages.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace OnHive.Storages.Api.Endpoints
{
    internal static class StorageFilesEndpoints
    {
        internal static WebApplication MapStorageFilesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Storage/File/{StorageId}", async (HttpContext context, [FromServices] IStorageFilesService service, [FromRoute] string storageId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(storageId);
                if (result == null) return Results.Ok(Response<StorageFileDto>.Empty());
                return Results.Ok(Response<StorageFileDto>.Ok(result));
            })
            .WithName("GetFileById")
            .WithDescription("Get Storage by Id")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Response<StorageFileDto>>();

            app.MapGet("v1/Storage/Files", async (HttpContext context, [FromServices] IStorageFilesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<StorageFileDto>>.Ok(result));
            })
            .WithName("GetFiles")
            .WithDescription("Get all Storages")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Response<PaginatedResult<StorageFileDto>>>();

            app.MapPut("v1/Storage/File", async (HttpContext context, [FromServices] IStorageFilesService service, [FromBody] StorageFileDto storageDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(storageDto, loggedUser);
                if (result == null) return Results.Ok(Response<StorageFileDto>.Empty());
                return Results.Ok(Response<StorageFileDto>.Ok(result));
            })
            .WithName("UpdateFile")
            .WithDescription("Update an Storage")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_update"))
            .Produces<Response<StorageFileDto>>();

            app.MapPost("v1/Storage/File", async (HttpContext context, [FromServices] IStorageFilesService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var file = context.Request.Form.Files.FirstOrDefault();
                if (file == null) return Results.BadRequest("File not found");
                context.Request.Form.TryGetValue("fileId", out var fileId);
                context.Request.Form.TryGetValue("name", out var name);
                context.Request.Form.TryGetValue("description", out var description);
                context.Request.Form.TryGetValue("tags", out var tags);
                context.Request.Form.TryGetValue("categories", out var categories);
                context.Request.Form.TryGetValue("public", out var publicFile);
                var storageDto = new StorageFileDto
                {
                    FileId = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}",
                    Name = name.ToString() ?? file.Name,
                    Description = description.ToString() ?? file.FileName,
                    OriginalFileName = file.FileName,
                    Type = Path.GetExtension(file.FileName),
                    Public = string.IsNullOrEmpty(publicFile.ToString()) || bool.Parse(publicFile.ToString()),
                    Tags = tags.Count > 0 ? tags.ToString().Split(',').ToList() : [],
                    Categories = categories.Count > 0 ? categories.ToString().Split(',').ToList() : []
                };
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var result = await service.UploadFileAsync(memoryStream, storageDto, loggedUser);
                if (result == null) return Results.Ok(Response<StorageFileDto>.Empty());
                return Results.Ok(Response<StorageFileDto>.Ok(result));
            })
            .WithName("UploadFile")
            .WithDescription("Upload File")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_create"))
            .Produces<Response<StorageFileDto>>();

            // app.MapPost("v1/Internal/Storage/File", async (HttpContext context, [FromServices] IStorageFilesService service, [FromQuery(Name = "target-folder")] string targetFolder = "files") =>
            // {
            //     var file = context.Request.Form.Files.FirstOrDefault();
            //     if (file == null) return Results.BadRequest("File not found");
            //     context.Request.Form.TryGetValue("fileId", out var fileId);
            //     context.Request.Form.TryGetValue("name", out var name);
            //     context.Request.Form.TryGetValue("description", out var description);
            //     context.Request.Form.TryGetValue("tags", out var tags);
            //     context.Request.Form.TryGetValue("categories", out var categories);
            //     context.Request.Form.TryGetValue("public", out var publicFile);
            //     context.Request.Form.TryGetValue("tenantId", out var tenantId);

            //     var storageDto = new StorageFileDto
            //     {
            //         TenantId = tenantId,
            //         FileId = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}",
            //         Name = name.ToString() ?? file.Name,
            //         Description = description.ToString() ?? file.FileName,
            //         OriginalFileName = file.FileName,
            //         Type = Path.GetExtension(file.FileName),
            //         Public = string.IsNullOrEmpty(publicFile.ToString()) || bool.Parse(publicFile.ToString()),
            //         Tags = tags.Count > 0 ? tags.ToString().Split(',').ToList() : [],
            //         Categories = categories.Count > 0 ? categories.ToString().Split(',').ToList() : []
            //     };
            //     var memoryStream = new MemoryStream();
            //     await file.CopyToAsync(memoryStream);
            //     var result = await service.UploadFileAsync(memoryStream, storageDto, targetFolder);
            //     if (result == null) return Results.Ok(Response<StorageFileDto>.Empty());
            //     return Results.Ok(Response<StorageFileDto>.Ok(result));
            // })
            //.WithName("UploadFileInternal")
            //.WithDescription("Upload File internal")
            //.WithTags("Internal")
            //.AllowAnonymous()
            //.Produces<Response<StorageFileDto>>();

            app.MapGet("v1/Storage/File/{tenantId}/{fileId}", async (HttpContext context, [FromServices] IStorageFilesService service, [FromRoute] string tenantId, [FromRoute] string fileId) =>
            {
                var result = await service.GetFileAsync(fileId, tenantId);
                return Results.File(result, ContentTypeHelper.GetContentType(fileId), fileId);
            })
            .WithName("GetFile")
            .WithDescription("Get File")
            .WithTags("Storages")
            .Produces<Stream>()
            .AllowAnonymous();

            app.MapGet("v1/Storage/PrivateFile/{fileId}", async (HttpContext context, [FromServices] IStorageFilesService service, [FromRoute] string fileId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.GetFileAsync(fileId, loggedUser);

                return Results.File(result, ContentTypeHelper.GetContentType(fileId), fileId);
            })
            .WithName("GetPrivateFile")
            .WithDescription("Get Private File")
            .WithTags("Storages")
            .WithMetadata(PermissionConfig.Create("storages_read"))
            .Produces<Stream>();

            app.MapDelete("v1/Storage/File/{fileId}", async (HttpContext context, [FromServices] IStorageFilesService service, [FromRoute] string fileId) =>
             {
                 var loggedUser = context.GetLoggedUser();
                 if (loggedUser?.User == null) return Results.Unauthorized();
                 await service.DeleteFileAsync(fileId, loggedUser);
                 return Results.Ok(Response<string>.Ok());
             })
             .WithName("DeleteFile")
             .WithDescription("Delete File")
             .WithTags("Storages")
             .WithMetadata(PermissionConfig.Create("storages_update"))
             .Produces<Response<string>>();

            return app;
        }
    }
}