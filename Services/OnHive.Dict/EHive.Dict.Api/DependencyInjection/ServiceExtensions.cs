using EHive.Dict.Domain.Abstractions.Repositories;
using EHive.Dict.Domain.Abstractions.Services;
using EHive.Dict.Domain.Mappers;
using EHive.Dict.Repositories;
using EHive.Dict.Services;

namespace EHive.Dict.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddDict(this IServiceCollection services)
        {
            return services
                .AddRepositories()
                .AddServices()
                .AddMappers();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IDictRepository, DictRepository>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IDictService, DictService>();
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}