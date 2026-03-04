using OnHive.Configuration.Api.Endpoints;

namespace OnHive.Configuration.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapConfigurationApi(this WebApplication app)
        {
            app.MapConfigurationEndpoints();
            return app;
        }
    }
}