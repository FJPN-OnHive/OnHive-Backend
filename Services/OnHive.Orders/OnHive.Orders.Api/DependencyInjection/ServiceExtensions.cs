using OnHive.Carts.Repositories;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Mappers;
using OnHive.Orders.Repositories;
using OnHive.Orders.Services;

namespace OnHive.Orders.Api.DependencyInjection
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