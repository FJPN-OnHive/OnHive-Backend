using OnHive.Configuration.Library.Extensions;
using OnHive.Certificates.Domain.Models;
using MongoDB.Driver;

namespace OnHive.Certificates.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureCertificatesApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<CertificatesApiSettings>();
            builder.Services.AddCertificates();
            return builder;
        }
    }
}