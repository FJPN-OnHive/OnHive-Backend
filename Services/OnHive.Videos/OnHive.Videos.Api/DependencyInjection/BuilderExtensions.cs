using OnHive.Configuration.Library.Extensions;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Events.Api.DependencyInjection;
using OnHive.Videos.Domain.Models;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;

namespace OnHive.Videos.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureVideosApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<VideosApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.CheckCredentials();
            builder.ConfigureEventRegister("Videos");
            builder.RegisterEvents();
            builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = long.MaxValue);
            builder.Services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = long.MaxValue;
            });
            return builder;
        }

        private static WebApplicationBuilder CheckCredentials(this WebApplicationBuilder builder)
        {
            var settings = builder.Services.BuildServiceProvider().GetRequiredService<VideosApiSettings>();
            if (settings != null && !string.IsNullOrEmpty(settings.CredentialsFile) && File.Exists(settings.CredentialsFile))
            {
                var file = File.ReadAllText(settings.CredentialsFile);
                var credentials = JsonSerializer.Deserialize<VideosApiSettings>(file);
                builder.Services.Remove(builder.Services.FirstOrDefault(x => x.ServiceType == typeof(VideosApiSettings)));
                builder.Services.AddSingleton(settings);
            }

            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.VideoUploaded,
                Message = "Event triggered when a video is uploaded",
                Origin = "Videos",
                Tags = new List<string> { "Video", "Uploaded" },
                Fields = new Dictionary<string, string> {
                    { "VideoId", "The id of the Video" },
                    { "FileName", "Video Original File name" },
                    { "VideoFileId", "Video Id on S3" },
                    { "VideoUrl", "The Url of processed Video" },
                    { "SourceVideoUrl", "The Url of processed Video" },
                    { "VideoStatus", "The Status of the Video" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.VideoProcessing,
                Message = "Event triggered when an video start processing",
                Origin = "Videos",
                Tags = new List<string> { "Video", "Processing" },
                Fields = new Dictionary<string, string>
                {
                   { "VideoId", "The id of the Video" },
                    { "FileName", "Video Original File name" },
                    { "VideoFileId", "Video Id on S3" },
                    { "VideoUrl", "The Url of processed Video" },
                    { "SourceVideoUrl", "The Url of processed Video" },
                    { "VideoStatus", "The Status of the Video" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.VideoProcessed,
                Message = "Event triggered when an Video is Processed successfully",
                Origin = "Videos",
                Tags = new List<string> { "Video", "Processed" },
                Fields = new Dictionary<string, string>
                {
                    { "VideoId", "The id of the Video" },
                    { "FileName", "Video Original File name" },
                    { "VideoFileId", "Video Id on S3" },
                    { "VideoUrl", "The Url of processed Video" },
                    { "SourceVideoUrl", "The Url of processed Video" },
                    { "VideoStatus", "The Status of the Video" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.VideoError,
                Message = "Event triggered when an Video is on error state",
                Origin = "Videos",
                Tags = new List<string> { "Video", "Error" },
                Fields = new Dictionary<string, string>
                {
                    { "VideoId", "The id of the Video" },
                    { "FileName", "Video Original File name" },
                    { "VideoFileId", "Video Id on S3" },
                    { "VideoUrl", "The Url of processed Video" },
                    { "SourceVideoUrl", "The Url of processed Video" },
                    { "VideoStatus", "The Status of the Video" }
                }
            });

            return builder;
        }
    }
}