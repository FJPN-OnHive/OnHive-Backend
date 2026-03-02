using EHive.Carts.Repositories;
using EHive.Orders.Domain.Abstractions.Repositories;
using EHive.Orders.Domain.Abstractions.Services;
using EHive.Orders.Domain.Mappers;
using EHive.Orders.Repositories;
using EHive.Orders.Services;

namespace EHive.Orders.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ICartsRepository, CartsRepository>()
                .AddTransient<IOrdersRepository, OrdersRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ICartsService, CartsService>()
                .AddTransient<IOrdersService, OrdersService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}