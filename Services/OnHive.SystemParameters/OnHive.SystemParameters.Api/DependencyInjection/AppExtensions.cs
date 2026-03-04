using OnHive.SystemParameters.Api.Endpoints;

namespace OnHive.SystemParameters.Api.DependencyInjection
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