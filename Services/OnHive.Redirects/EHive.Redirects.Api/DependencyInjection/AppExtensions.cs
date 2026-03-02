using EHive.Redirects.Api.Endpoints;

namespace EHive.Redirects.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapRedirectsApi(this WebApplication app)
        {
            app.MapRedirectEndpoints();
            return app;
        }
    }
}