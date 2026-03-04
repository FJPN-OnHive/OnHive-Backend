using OnHive.Configuration.Library.Extensions;
using OnHive.Tenants.Domain.Models;

namespace OnHive.Tenants.Api.DependencyInjection
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