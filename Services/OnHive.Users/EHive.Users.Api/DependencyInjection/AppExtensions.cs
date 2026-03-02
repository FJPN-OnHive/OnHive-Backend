using EHive.Users.Api.Endpoints;

namespace EHive.Users.Api.DependencyInjection
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