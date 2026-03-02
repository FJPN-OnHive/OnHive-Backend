using EHive.Configuration.Api.Endpoints;

namespace EHive.Configuration.Api.DependencyInjection
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