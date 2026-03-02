using EHive.SystemParameters.Api.Endpoints;

namespace EHive.SystemParameters.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapSystemParametersApi(this WebApplication app)
        {
            app.MapSystemParametersEndpoints();
            return app;
        }
    }
}