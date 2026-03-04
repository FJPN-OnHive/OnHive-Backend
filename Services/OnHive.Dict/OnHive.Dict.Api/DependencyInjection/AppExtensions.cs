using OnHive.Dict.Api.Endpoints;

namespace OnHive.Dict.Api.DependencyInjection
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