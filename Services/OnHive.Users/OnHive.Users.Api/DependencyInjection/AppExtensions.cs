using OnHive.Users.Api.Endpoints;

namespace OnHive.Users.Api.DependencyInjection
{
    public static class AppExtensions
    {
        public static WebApplication MapUsersApi(this WebApplication app)
        {
            app.MapUsersEndpoints();
            app.MapUserGroupsEndpoints();
            app.MapLoginEndpoints();
            app.MapRolesEndpoints();
            app.MapUserProfilesEndpoints();
            return app;
        }
    }
}