using EHive.Configuration.Library.Extensions;
using EHive.SystemParameters.Domain.Models;

namespace EHive.SystemParameters.Api.DependencyInjection
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