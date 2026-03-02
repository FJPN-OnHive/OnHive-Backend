using AutoMapper;
using EHive.Core.Library.Contracts.Orders;
using EHive.Core.Library.Entities.Orders;

namespace EHive.Orders.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapOrderToOrderDto();
            MapCartToCartDto();
        }

        private void MapOrderToOrderDto()
        {
            CreateMap<OrderItem, OrderItemDto>()
                .ReverseMap();

            CreateMap<Order, OrderDto>()
                .ReverseMap();
        }

        private void MapCartToCartDto()
        {
            CreateMap<CartItem, CartItemDto>()
                .ReverseMap();

            CreateMap<Cart, CartDto>()
                .ReverseMap();
        }
    }
}