using EHive.Redirects.Domain.Abstractions.Repositories;
using EHive.Redirects.Domain.Abstractions.Services;
using EHive.Redirects.Domain.Mappers;
using EHive.Redirects.Repositories;
using EHive.Redirects.Services;

namespace EHive.Redirects.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IRedirectRepository, RedirectRepository>(); 
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IRedirectService, RedirectService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}
