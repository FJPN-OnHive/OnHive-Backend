using EHive.Posts.Api.Endpoints;

namespace EHive.Posts.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapPostsApi(this WebApplication app)
        {
            app.MapPostsEndpoints();
            return app;
        }
    }
}