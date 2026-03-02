using EHive.Certificates.Api.Endpoints;

namespace EHive.Certificates.Api.DependencyInjection
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