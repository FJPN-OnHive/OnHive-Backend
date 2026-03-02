using EHive.Certificates.Domain.Abstractions.Repositories;
using EHive.Certificates.Domain.Abstractions.Services;
using EHive.Certificates.Domain.Mappers;
using EHive.Certificates.Repositories;
using EHive.Certificates.Services;

namespace EHive.Certificates.Api.DependencyInjection
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