using OnHive.Payments.Api.Endpoints;

namespace OnHive.Payments.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapPaymentsApi(this WebApplication app)
        {
            app.MapPaymentEndpoints();
            app.MapBankSlipSettingsesEndpoints();
            return app;
        }
    }
}