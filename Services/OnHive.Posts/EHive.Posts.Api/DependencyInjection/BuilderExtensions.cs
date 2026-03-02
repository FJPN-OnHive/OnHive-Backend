using EHive.Configuration.Library.Extensions;
using EHive.Posts.Domain.Models;

namespace EHive.Posts.Api.DependencyInjection
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