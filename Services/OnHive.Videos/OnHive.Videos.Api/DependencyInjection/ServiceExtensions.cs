using OnHive.Videos.Domain.Abstractions.Repositories;
using OnHive.Videos.Domain.Abstractions.Services;
using OnHive.Videos.Domain.Mappers;
using OnHive.Videos.Repositories;
using OnHive.Videos.Services;

namespace OnHive.Videos.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IVideosRepository, VideosRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IVideosService, VideosService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}