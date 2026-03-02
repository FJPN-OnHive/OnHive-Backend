using EHive.Teachers.Api.Endpoints;

namespace EHive.Teachers.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapTeachersApi(this WebApplication app)
        {
            app.MapTeachersEndpoints();
            return app;
        }
    }
}