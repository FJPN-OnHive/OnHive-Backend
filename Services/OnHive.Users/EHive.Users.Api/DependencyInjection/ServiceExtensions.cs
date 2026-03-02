using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Users.Domain.Mappers;
using EHive.Users.Repositories;
using EHive.Users.Services;

namespace EHive.Users.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IUsersRepository, UsersRepository>()
                .AddTransient<IRolesRepository, RolesRepository>()
                .AddTransient<IUserGroupsRepository, UserGroupsRepository>()
                .AddTransient<IUserProfilesRepository, UserProfilesRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IUsersService, UsersService>()
                .AddTransient<IRolesService, RolesService>()
                .AddTransient<ILoginService, LoginService>()
                .AddTransient<IUserGroupsService, UserGroupsService>()
                .AddTransient<IUserProfilesService, UserProfilesService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}