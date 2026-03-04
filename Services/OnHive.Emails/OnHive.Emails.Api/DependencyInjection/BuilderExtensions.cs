using OnHive.Configuration.Library.Extensions;
using OnHive.Emails.Domain.Models;

namespace OnHive.Emails.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureEmailsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<EmailsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}