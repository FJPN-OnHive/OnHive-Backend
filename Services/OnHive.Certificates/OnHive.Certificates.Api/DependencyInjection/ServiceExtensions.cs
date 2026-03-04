using OnHive.Certificates.Domain.Abstractions.Repositories;
using OnHive.Certificates.Domain.Abstractions.Services;
using OnHive.Certificates.Domain.Mappers;
using OnHive.Certificates.Repositories;
using OnHive.Certificates.Services;

namespace OnHive.Certificates.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCertificates(this IServiceCollection services)
        {
            return services
                .AddRepositories()
                .AddServices()
                .AddMappers();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ICertificatesRepository, CertificatesRepository>()
                .AddTransient<ICertificateMountsRepository, CertificateMountsRepository>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ICertificatesService, CertificatesService>();
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}