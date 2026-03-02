using EHive.Orders.Api.Endpoints;

namespace EHive.Orders.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapOrdersApi(this WebApplication app)
        {
            app.MapOrdersEndpoints();
            app.MapCartsEndpoints();
            return app;
        }
    }
}