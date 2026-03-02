using EHive.Configuration.Library.Extensions;
using EHive.Dict.Domain.Models;

namespace EHive.Dict.Api.DependencyInjection
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