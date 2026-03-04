using OnHive.Search.Api.Endpoints;

namespace OnHive.Search.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapSearchApi(this WebApplication app)
        {
            app.MapSearchEndpoints();
            app.MapProductCourseSearchEndpoints();
            return app;
        }
    }
}