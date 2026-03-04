using OnHive.Posts.Api.Endpoints;

namespace OnHive.Posts.Api.DependencyInjection
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