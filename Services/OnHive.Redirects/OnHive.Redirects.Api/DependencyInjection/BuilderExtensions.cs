using OnHive.Configuration.Library.Extensions;
using OnHive.Redirects.Domain.Models;

namespace OnHive.Redirects.Api.DependencyInjection
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