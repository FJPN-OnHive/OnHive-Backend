using OnHive.Configuration.Library.Extensions;
using OnHive.Teachers.Domain.Models;

namespace OnHive.Teachers.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureTeachersApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<TeachersApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}