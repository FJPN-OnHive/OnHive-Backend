using Microsoft.Extensions.DependencyInjection;
using OnHive.Domains.Common.Helpers;

namespace OnHive.Domains.Common.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection SetServiceProviderHelper(this IServiceCollection services)
        {
            ServiceProviderFactory.SetServiceProvider(services.BuildServiceProvider());
            return services;
        }
    }
}