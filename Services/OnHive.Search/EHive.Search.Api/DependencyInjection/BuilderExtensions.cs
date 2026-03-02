using EHive.Configuration.Library.Extensions;
using EHive.Search.Domain.Models;

namespace EHive.Search.Api.DependencyInjection
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