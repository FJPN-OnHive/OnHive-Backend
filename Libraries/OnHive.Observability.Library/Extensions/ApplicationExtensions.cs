using EHive.Observability.Library.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EHive.Configuration.Library.Extensions
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