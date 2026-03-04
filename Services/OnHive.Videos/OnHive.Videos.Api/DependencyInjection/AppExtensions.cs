using OnHive.Videos.Api.Endpoints;

namespace OnHive.Videos.Api.DependencyInjection
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