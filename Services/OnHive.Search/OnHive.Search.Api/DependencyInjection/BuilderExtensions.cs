using OnHive.Configuration.Library.Extensions;
using OnHive.Search.Domain.Models;

namespace OnHive.Search.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureSearchApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<SearchApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}