using OnHive.Configuration.Library.Extensions;
using OnHive.Catalog.Domain.Models;

namespace OnHive.Catalog.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureProductsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<CatalogApiSettings>();
            builder.Services.AddProducts();
            return builder;
        }
    }
}