using EHive.Configuration.Library.Extensions;
using EHive.Emails.Domain.Models;

namespace EHive.Emails.Api.DependencyInjection
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