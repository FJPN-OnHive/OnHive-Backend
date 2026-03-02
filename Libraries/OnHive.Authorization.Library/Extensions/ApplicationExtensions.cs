using EHive.Authorization.Library.Middlewares;
using EHive.Configuration.Library.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EHive.Authorization.Library.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddAuthentication(this WebApplication app)
        {
            app.UseMiddleware<PermissionsMiddleware>();
            return app;
        }

        public static WebApplication AddCorsCofiguration(this WebApplication app)
        {
            app.UseCors(option =>
            {
                var settings = app.Services.GetRequiredService<EnvironmentSettings>();
                option.WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");
                option.AllowAnyHeader();
                option.AllowAnyOrigin();
            });
            return app;
        }
    }
}