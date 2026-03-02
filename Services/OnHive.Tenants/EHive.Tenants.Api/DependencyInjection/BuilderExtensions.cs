using EHive.Configuration.Library.Extensions;
using EHive.Tenants.Domain.Models;

namespace EHive.Tenants.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureTenantsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<TenantsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}