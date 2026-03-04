using OnHive.Configuration.Library.Extensions;
using OnHive.Emails.Domain.Models;

namespace OnHive.Configuration.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureConfigurationApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<EmailsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}