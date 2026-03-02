using EHive.Configuration.Domain.Abstractions.Services;
using EHive.Configuration.Library.Exceptions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Configuration;
using EHive.Core.Library.Entities.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace EHive.Configuration.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder AddConfigurationSources(this WebApplicationBuilder builder, string configurationAppKey = "")
        {
            builder.Configuration.AddEnvironmentVariables();
            builder.AddConfiguration<EnvironmentSettings>();
            builder.AddConfiguration<CompanyInfo>();
            builder.AddConfiguration<ProjectInfo>();
            return builder;
        }

        public static WebApplicationBuilder ConfigureOpenApi(this WebApplicationBuilder builder, string version = "v1", bool includeAuthorization = true)
        {
            var projectInfo = builder.Services.BuildServiceProvider().GetService<ProjectInfo>();
            var title = System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName.Replace(".exe", "");
            var description = System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName.Replace(".exe", "").Replace(".", " ");
            if (projectInfo != null)
            {
                title = projectInfo?.ProjectName;
                description = projectInfo?.ProjectDescription;
            }
            if (File.Exists("docs.txt"))
            {
                description = File.ReadAllText("docs.txt");
            }
            else if (File.Exists("docs.md"))
            {
                description = File.ReadAllText("docs.md");
            }
            return builder.ConfigureOpenApi(title ?? "", description ?? "", version, includeAuthorization);
        }

        public static WebApplicationBuilder ConfigureOpenApi(this WebApplicationBuilder builder, string title, string version = "v1", bool includeAuthorization = true)
        {
            var projectInfo = builder.Services.BuildServiceProvider().GetService<ProjectInfo>();
            var description = System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName.Replace(".exe", "").Replace(".", " ");
            if (projectInfo != null)
            {
                description = projectInfo?.ProjectDescription;
            }
            if (File.Exists("docs.txt"))
            {
                description = File.ReadAllText("docs.txt");
            }
            else if (File.Exists("docs.md"))
            {
                description = File.ReadAllText("docs.md");
            }
            return builder.ConfigureOpenApi(title ?? "", description ?? "", version, includeAuthorization);
        }

        public static WebApplicationBuilder ConfigureOpenApi(this WebApplicationBuilder builder, string title, string description, string version = "v1", bool includeAuthorization = true)
        {
            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JSON Web Token based security",
            };

            var securityReq = new OpenApiSecurityRequirement()
            {
            };

            var info = new OpenApiInfo()
            {
                Version = version,
                Title = title,
                Description = description
            };

            var companyInfo = builder.Services.BuildServiceProvider().GetService<CompanyInfo>();
            if (companyInfo != null)
            {
                var contact = new OpenApiContact()
                {
                    Name = companyInfo?.ContactName,
                    Email = companyInfo?.ContactEmail
                };
                if (Uri.TryCreate(companyInfo?.ContactUrl, new UriCreationOptions(), out var contactUrl))
                {
                    contact.Url = contactUrl;
                }
                info.Contact = contact;
            }

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddOpenApi(o =>
            {
                o.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                if (includeAuthorization)
                {
                    o.AddDocumentTransformer(new BearerSecuritySchemeTransformer());
                }
                o.AddOperationTransformer(new PermissionsOperationFilter());
            });

            return builder;
        }

        public static WebApplicationBuilder AddConfiguration<T>(this WebApplicationBuilder builder)
            where T : class, new()
        {
            if (builder.Configuration.GetSection(typeof(T).Name).Value == null && builder.Configuration.GetSection(typeof(T).Name).GetChildren().Count() == 0)
            {
                builder.Services.AddSingleton<T>(serviceProvider => GetSettings<T>(serviceProvider));
            }
            else
            {
                var settings = new T();
                builder.Configuration.Bind(typeof(T).Name, settings);
                ValidateFields(settings);
                builder.Services.AddSingleton(settings);
            }
            return builder;
        }

        private static T GetSettings<T>(IServiceProvider serviceProvider) where T : class, new()

        {
            var settings = new T();
            var configService = serviceProvider.GetService<IConfigurationService>();
            if (configService != null)
            {
                var configGroup = configService.GetByTypeAsync<T>().GetAwaiter().GetResult();
                if (configGroup != null)
                {
                    try
                    {
                        settings = configGroup.Value as T ?? new T();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error deserializing configuration for {ConfigKey}", typeof(T).Name);
                        throw new MissingConfigurationException<T>($"Error deserializing configuration for {typeof(T).Name}: {ex.Message}");
                    }
                }
                else
                {
                    var configItem = new ConfigItemDto()
                    {
                        Key = typeof(T).Name,
                        Value = settings,
                        Description = $"Configuration group for {typeof(T).Name}"
                    };
                    configService.SaveAsync(configItem, null).GetAwaiter().GetResult();
                }
            }
            return settings;
        }

        private static void ValidateFields<T>(T settings) where T : class, new()
        {
            var context = new ValidationContext(settings);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(settings, context, results))
            {
                throw new MissingConfigurationException<T>(string.Join(", ", results.Select(x => x.ErrorMessage)));
            }
        }
    }
}