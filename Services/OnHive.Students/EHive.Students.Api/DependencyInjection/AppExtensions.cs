using EHive.StudentReports.Api.Endpoints;
using EHive.Students.Api.Endpoints;

namespace EHive.Students.Api.DependencyInjection
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