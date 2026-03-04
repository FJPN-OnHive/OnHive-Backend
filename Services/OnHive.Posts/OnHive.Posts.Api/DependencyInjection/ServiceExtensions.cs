using OnHive.Posts.Domain.Abstractions.Repositories;
using OnHive.Posts.Domain.Abstractions.Services;
using OnHive.Posts.Domain.Mappers;
using OnHive.Posts.Repositories;
using OnHive.Posts.Services;

namespace OnHive.Posts.Api.DependencyInjection
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