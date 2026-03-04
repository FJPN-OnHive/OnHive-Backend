using AutoMapper;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Entities.Catalog;

namespace OnHive.Catalog.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapProductToProductDto();
            MapCouponToCouponDto();
        }

        private void MapProductToProductDto()
        {
            CreateMap<Installment, InstallmentDto>()
                .ReverseMap();

            CreateMap<ProductPrice, ProductPriceDto>()
                .ReverseMap();

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.SpecialDiscount,
                    opt => opt.MapFrom(src => src.Prices.Any(p => p.ActivePromo && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                    ? new SpecialDiscount
                    {
                        Active = true,
                        Value = src.Prices.Where(p => p.ActivePromo && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now).MinBy(v => v.Price).Discount == 0
                        ? 100 - ((src.Prices.Where(p => p.ActivePromo && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now).Min(p => p.Price) / src.FullPrice) * 100)
                        : src.Prices.Where(p => p.ActivePromo && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now).MinBy(v => v.Price).Discount
                    }
                    : new SpecialDiscount { Active = false, Value = 0 }))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Sells, opt => opt.MapFrom(src => src.Sales))
                .ReverseMap();
        }

        private void MapCouponToCouponDto()
        {
            CreateMap<Coupon, CouponDto>()
                .ReverseMap();
        }
    }
}