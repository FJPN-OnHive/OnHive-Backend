using EHive.Configuration.Library.Extensions;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Api.DependencyInjection;
using EHive.Storages.Domain.Models;
using System.Text.Json;

namespace EHive.Storages.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureStoragesApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<StoragesApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.CheckCredentials();
            builder.ConfigureEventRegister("Storages");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder CheckCredentials(this WebApplicationBuilder builder)
        {
            var settings = builder.Services.BuildServiceProvider().GetRequiredService<StoragesApiSettings>();
            if (settings != null && !string.IsNullOrEmpty(settings.CredentialsFile) && File.Exists(settings.CredentialsFile))
            {
                var file = File.ReadAllText(settings.CredentialsFile);
                var credentials = JsonSerializer.Deserialize<StoragesApiSettings>(file);
                settings.BucketSecret = credentials?.BucketSecret;
                settings.BucketKeyId = credentials?.BucketKeyId;
                builder.Services.Remove(builder.Services.FirstOrDefault(x => x.ServiceType == typeof(StoragesApiSettings)));
                builder.Services.AddSingleton(settings);
            }

            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.ImageUploaded,
                Message = "Event triggered when a image is uploaded",
                Origin = "Storages",
                Tags = new List<string> { "Storages", "Image", "Uploaded" },
                Fields = new Dictionary<string, string> {
                    { "ImageId", "image Id" },
                    { "FileName", "image Original File Name" },
                    { "ImageFileId", "Image file Id" },
                    { "HiResImage", "HiRes Image Url" },
                    { "MidResImage", "MidRes Image Url" },
                    { "LowResImage", "LowRes Image Url" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.ImageError,
                Message = "Event triggered when an image fails",
                Origin = "Storages",
                Tags = new List<string> { "Storages", "Image", "Error" },
                Fields = new Dictionary<string, string>
                {
                    { "ImageId", "image Id" },
                    { "FileName", "image Original File Name" },
                    { "ImageFileId", "Image file Id" },
                    { "HiResImage", "HiRes Image Url" },
                    { "MidResImage", "MidRes Image Url" },
                    { "LowResImage", "LowRes Image Url" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.FileUploaded,
                Message = "Event triggered when a file is uploaded",
                Origin = "Storages",
                Tags = new List<string> { "Storages", "File", "Uploaded" },
                Fields = new Dictionary<string, string> {
                    { "Id", "file Id" },
                    { "FileName", "image Original File Name" },
                    { "FileId", "External File Id" },
                    { "FileUrl", "File Url" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.FileError,
                Message = "Event triggered when an file fails",
                Origin = "Storages",
                Tags = new List<string> { "Storages", "File", "Error" },
                Fields = new Dictionary<string, string>
                {
                     { "Id", "file Id" },
                    { "FileName", "image Original File Name" },
                    { "FileId", "External File Id" },
                    { "FileUrl", "File Url" }
                }
            });

            return builder;
        }
    }
}