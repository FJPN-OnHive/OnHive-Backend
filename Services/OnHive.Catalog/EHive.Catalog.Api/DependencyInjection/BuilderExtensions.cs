using EHive.Configuration.Library.Extensions;
using EHive.Catalog.Domain.Models;

namespace EHive.Catalog.Api.DependencyInjection
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