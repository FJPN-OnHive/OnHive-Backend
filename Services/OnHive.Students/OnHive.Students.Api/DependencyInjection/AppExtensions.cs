using OnHive.StudentReports.Api.Endpoints;
using OnHive.Students.Api.Endpoints;

namespace OnHive.Students.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapStudentsApi(this WebApplication app)
        {
            app.MapStudentsEndpoints();
            app.MapStudentActivitiesEndpoints();
            app.MapStudentReportsEndpoints();
            return app;
        }
    }
}