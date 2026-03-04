using OnHive.Observability.Library.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OnHive.Configuration.Library.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddTracing(this WebApplication app)
        {
            app.UseMiddleware<TracingMiddleware>();
            return app;
        }
    }
}