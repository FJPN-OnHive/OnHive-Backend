using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Mappers;
using OnHive.Tenants.Repositories;
using OnHive.Tenants.Services;

namespace OnHive.Tenants.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ITenantsRepository, TenantsRepository>()
                .AddTransient<ITenantParametersRepository, TenantParametersRepository>()
                .AddTransient<IFeaturesRepository, FeaturesRepository>()
                .AddTransient<ITenantThemesRepository, TenantThemesRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ITenantsService, TenantsService>()
                .AddTransient<ITenantParametersService, TenantParametersService>()
                .AddTransient<IFeaturesService, FeaturesService>()
                .AddTransient<ITenantThemesService, TenantThemesService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}