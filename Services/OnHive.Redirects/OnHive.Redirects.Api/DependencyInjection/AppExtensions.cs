using OnHive.Redirects.Api.Endpoints;

namespace OnHive.Redirects.Api.DependencyInjection
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