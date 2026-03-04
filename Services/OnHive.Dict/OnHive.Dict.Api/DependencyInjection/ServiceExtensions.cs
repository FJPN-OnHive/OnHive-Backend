using OnHive.Dict.Domain.Abstractions.Repositories;
using OnHive.Dict.Domain.Abstractions.Services;
using OnHive.Dict.Domain.Mappers;
using OnHive.Dict.Repositories;
using OnHive.Dict.Services;

namespace OnHive.Dict.Api.DependencyInjection
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