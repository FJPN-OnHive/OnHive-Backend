using EHive.SystemParameters.Domain.Abstractions.Repositories;
using EHive.SystemParameters.Domain.Abstractions.Services;
using EHive.SystemParameters.Domain.Mappers;
using EHive.SystemParameters.Repositories;
using EHive.SystemParameters.Services;

namespace EHive.SystemParameters.Api.DependencyInjection
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
