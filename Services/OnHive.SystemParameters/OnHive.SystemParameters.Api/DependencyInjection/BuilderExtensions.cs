using OnHive.Configuration.Library.Extensions;
using OnHive.SystemParameters.Domain.Models;

namespace OnHive.SystemParameters.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureSystemParametersApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<SystemParametersApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}