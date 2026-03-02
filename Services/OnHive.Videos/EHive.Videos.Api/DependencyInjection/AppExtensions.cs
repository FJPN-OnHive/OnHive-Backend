using EHive.Videos.Api.Endpoints;

namespace EHive.Videos.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapVideosApi(this WebApplication app)
        {
            app.MapVideosEndpoints();
            return app;
        }
    }
}