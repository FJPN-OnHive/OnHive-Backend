using EHive.Events.Api.Endpoints;

namespace EHive.Events.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapEventsApi(this WebApplication app)
        {
            app.MapEventsEndpoints();
            app.MapAutomationsEndpoints();
            app.MapEventsConfigEndpoints();
            app.MapWebHooksEndpoints();
            app.MapIntegrationsEndpoints();
            return app;
        }
    }
}