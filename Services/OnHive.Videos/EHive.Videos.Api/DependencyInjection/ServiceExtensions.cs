using EHive.Videos.Domain.Abstractions.Repositories;
using EHive.Videos.Domain.Abstractions.Services;
using EHive.Videos.Domain.Mappers;
using EHive.Videos.Repositories;
using EHive.Videos.Services;

namespace EHive.Videos.Api.DependencyInjection
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