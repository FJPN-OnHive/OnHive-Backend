using Microsoft.Extensions.DependencyInjection;
using OnHive.Domains.Common.Abstractions.Services;
using OnHive.Domains.Common.Helpers;
using OnHive.Domains.Common.Services;

namespace OnHive.Domains.Common.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection SetServiceProviderHelper(this IServiceCollection services)
        {
            ServiceProviderFactory.SetServiceProvider(services.BuildServiceProvider());
            return services;
        }

        public static IServiceCollection SetServicesHub(this IServiceCollection services)
        {
            services.AddTransient<IServicesHub, ServicesHub>();
            return services;
        }
    }
}