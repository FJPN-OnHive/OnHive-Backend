using OnHive.Redirects.Domain.Abstractions.Repositories;
using OnHive.Redirects.Domain.Abstractions.Services;
using OnHive.Redirects.Domain.Mappers;
using OnHive.Redirects.Repositories;
using OnHive.Redirects.Services;

namespace OnHive.Redirects.Api.DependencyInjection
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
