using EHive.Posts.Domain.Abstractions.Repositories;
using EHive.Posts.Domain.Abstractions.Services;
using EHive.Posts.Domain.Mappers;
using EHive.Posts.Repositories;
using EHive.Posts.Services;

namespace EHive.Posts.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IPostsRepository, PostsRepository>()
                .AddTransient<IPostBackupRepository, PostBackupRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IPostsService, PostsService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}