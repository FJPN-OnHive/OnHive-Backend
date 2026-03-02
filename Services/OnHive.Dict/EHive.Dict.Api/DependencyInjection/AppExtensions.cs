using EHive.Dict.Api.Endpoints;

namespace EHive.Dict.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapDictApi(this WebApplication app)
        {
            app.MapDictEndpoints();
            return app;
        }
    }
}