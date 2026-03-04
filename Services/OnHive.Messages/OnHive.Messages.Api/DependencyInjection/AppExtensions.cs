using OnHive.Messages.Api.Endpoints;

namespace OnHive.Messages.Api.DependencyInjection
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