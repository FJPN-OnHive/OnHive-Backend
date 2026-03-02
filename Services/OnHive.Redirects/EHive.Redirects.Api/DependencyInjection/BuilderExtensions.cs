using EHive.Configuration.Library.Extensions;
using EHive.Redirects.Domain.Models;

namespace EHive.Redirects.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureRedirectApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<RedirectApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}