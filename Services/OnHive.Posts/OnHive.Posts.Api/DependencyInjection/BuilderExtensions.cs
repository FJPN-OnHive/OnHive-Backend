using OnHive.Configuration.Library.Extensions;
using OnHive.Posts.Domain.Models;

namespace OnHive.Posts.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigurePostsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<PostsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();

            return builder;
        }
    }
}