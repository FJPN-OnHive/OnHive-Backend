using EHive.Emails.Api.Endpoints;

namespace EHive.Emails.Api.DependencyInjection
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