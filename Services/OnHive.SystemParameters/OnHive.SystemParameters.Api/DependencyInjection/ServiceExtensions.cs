using OnHive.SystemParameters.Domain.Abstractions.Repositories;
using OnHive.SystemParameters.Domain.Abstractions.Services;
using OnHive.SystemParameters.Domain.Mappers;
using OnHive.SystemParameters.Repositories;
using OnHive.SystemParameters.Services;

namespace OnHive.SystemParameters.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ISystemParametersRepository, SystemParametersRepository>(); 
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ISystemParametersService, SystemParametersService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}
