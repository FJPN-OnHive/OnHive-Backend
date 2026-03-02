using EHive.Messages.Api.Endpoints;

namespace EHive.Messages.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapMessagesApi(this WebApplication app)
        {
            app.MapMessagesEndpoints();
            app.MapMessageChannelsEndpoints();
            return app;
        }
    }
}