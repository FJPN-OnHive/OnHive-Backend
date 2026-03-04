using OnHive.Storages.Api.Endpoints;

namespace OnHive.Storages.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapStoragesApi(this WebApplication app)
        {
            app.MapStorageFilesEndpoints();
            app.MapStorageImagesEndpoints();
            return app;
        }
    }
}