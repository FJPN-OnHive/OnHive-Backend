using EHive.Search.Domain.Abstractions.Repositories;
using EHive.Search.Domain.Abstractions.Services;
using EHive.Search.Domain.Mappers;
using EHive.Search.Repositories;
using EHive.Search.Services;

namespace EHive.Search.Api.DependencyInjection
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