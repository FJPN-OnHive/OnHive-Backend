using EHive.Invoices.Api.Endpoints;

namespace EHive.Invoices.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapInvoicesApi(this WebApplication app)
        {
            app.MapInvoicesEndpoints();
            return app;
        }
    }
}