using EHive.Search.Api.Endpoints;

namespace EHive.Search.Api.DependencyInjection
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