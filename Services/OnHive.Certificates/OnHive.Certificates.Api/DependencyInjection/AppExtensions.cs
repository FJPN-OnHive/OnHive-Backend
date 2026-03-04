using OnHive.Certificates.Api.Endpoints;

namespace OnHive.Certificates.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapCertificatesApi(this WebApplication app)
        {
            app.MapCertificatesEndpoints();
            return app;
        }
    }
}