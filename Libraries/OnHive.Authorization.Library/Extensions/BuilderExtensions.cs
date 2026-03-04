using OnHive.Authorization.Library.Models;
using OnHive.Configuration.Library.Extensions;
using OnHive.Configuration.Library.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OnHive.Authorization.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });
            return builder;
        }

        public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
        {
            var envSettings = builder.Services.BuildServiceProvider().GetService<EnvironmentSettings>();
            builder.AddConfiguration<AuthSettings>();
            builder.Services.AddEndpointsApiExplorer();
            return builder;
        }
    }
}