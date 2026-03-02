using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Catalog.Domain.Abstractions.Services;
using EHive.Catalog.Domain.Mappers;
using EHive.Catalog.Repositories;
using EHive.Catalog.Services;

namespace EHive.Catalog.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddProducts(this IServiceCollection services)
        {
            return services
                .AddRepositories()
                .AddServices()
                .AddMappers();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IProductsRepository, ProductsRepository>()
                .AddTransient<IUserCouponsRepository, UserCouponsRepository>()
                .AddTransient<ICouponsRepository, CouponsRepository>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IProductsService, ProductsService>()
                .AddTransient<ICouponsService, CouponsService>();
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}