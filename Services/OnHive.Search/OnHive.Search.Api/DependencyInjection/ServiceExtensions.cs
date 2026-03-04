using OnHive.Search.Domain.Abstractions.Repositories;
using OnHive.Search.Domain.Abstractions.Services;
using OnHive.Search.Domain.Mappers;
using OnHive.Search.Repositories;
using OnHive.Search.Services;

namespace OnHive.Search.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ISearchRepository, SearchRepository>()
                .AddTransient<IProductCourseSearchRepository, ProductCourseSearchRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ISearchService, SearchService>()
                .AddTransient<IProductCourseSearchService, ProductCourseSearchService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}