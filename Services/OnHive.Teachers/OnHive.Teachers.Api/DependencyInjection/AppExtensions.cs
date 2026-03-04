using OnHive.Teachers.Api.Endpoints;

namespace OnHive.Teachers.Api.DependencyInjection
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