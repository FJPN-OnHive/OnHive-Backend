using OnHive.Catalog.Domain.Mappers;
using OnHive.Configuration.Domain.Abstractions.Repositories;
using OnHive.Configuration.Domain.Abstractions.Services;
using OnHive.Configuration.Repositories;
using OnHive.Configuration.Services;

namespace OnHive.Configuration.Api.DependencyInjection
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