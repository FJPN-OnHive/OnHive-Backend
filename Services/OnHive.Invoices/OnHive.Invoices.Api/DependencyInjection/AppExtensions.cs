using OnHive.Invoices.Api.Endpoints;

namespace OnHive.Invoices.Api.DependencyInjection
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