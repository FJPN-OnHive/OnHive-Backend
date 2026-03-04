using OnHive.Catalog.Api.Endpoints;

namespace OnHive.Catalog.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapCatalogApi(this WebApplication app)
        {
            app.MapProductsEndpoints();
            app.MapCouponsEndpoints();
            return app;
        }
    }
}