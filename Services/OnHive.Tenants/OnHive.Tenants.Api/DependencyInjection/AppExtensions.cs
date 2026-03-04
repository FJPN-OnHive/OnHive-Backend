using OnHive.Tenants.Api.Endpoints;

namespace OnHive.Tenants.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapTenantsApi(this WebApplication app)
        {
            app.MapTenantsEndpoints();
            app.MapTenantParametersEndpoints();
            app.MapFeaturesEndpoints();
            app.MapTenantThemesEndpoints();
            return app;
        }
    }
}