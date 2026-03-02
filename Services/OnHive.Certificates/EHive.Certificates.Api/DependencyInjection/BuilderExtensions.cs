using EHive.Configuration.Library.Extensions;
using EHive.Certificates.Domain.Models;
using MongoDB.Driver;

namespace EHive.Certificates.Api.DependencyInjection
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