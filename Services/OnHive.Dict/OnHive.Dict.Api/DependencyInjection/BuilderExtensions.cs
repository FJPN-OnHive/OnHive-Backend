using OnHive.Configuration.Library.Extensions;
using OnHive.Dict.Domain.Models;

namespace OnHive.Dict.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureDictApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<DictApiSettings>();
            builder.Services.AddDict();
            return builder;
        }
    }
}