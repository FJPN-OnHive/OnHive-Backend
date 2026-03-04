using OnHive.Emails.Api.Endpoints;

namespace OnHive.Emails.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapEmailsApi(this WebApplication app)
        {
            app.MapEmailsEndpoints();
            return app;
        }
    }
}