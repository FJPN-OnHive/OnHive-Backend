using OnHive.Configuration.Library.Extensions;
using OnHive.Messages.Domain.Models;

namespace OnHive.Messages.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureMessagesApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<MessagesApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.Services.AddHttpClient();
            return builder;
        }
    }
}