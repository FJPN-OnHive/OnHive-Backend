using EHive.Courses.Api.Endpoints;

namespace EHive.Courses.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapCoursesApi(this WebApplication app)
        {
            app.MapCoursesEndpoints();
            app.MapDisciplineEndpoints();
            app.MapLessonsEndpoints();
            app.MapExamsEndpoints();
            return app;
        }
    }
}