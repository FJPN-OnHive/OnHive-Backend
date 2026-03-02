using Microsoft.Extensions.DependencyInjection;

namespace OnHive.Domains.Common.Helpers
{
    public static class ServiceProviderFactory
    {
        private static ServiceProvider? serviceProvider;

        public static ServiceProvider? ServiceProvider => serviceProvider;

        internal static void SetServiceProvider(ServiceProvider provider)
        {
            serviceProvider = provider;
        }
    }
}