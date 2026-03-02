using EHive.Catalog.Domain.Mappers;
using EHive.Configuration.Domain.Abstractions.Repositories;
using EHive.Configuration.Domain.Abstractions.Services;
using EHive.Configuration.Repositories;
using EHive.Configuration.Services;

namespace EHive.Configuration.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IConfigurationRepository, ConfigurationRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IConfigurationService, ConfigurationService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}