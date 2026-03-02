using AutoMapper;
using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Entities.Payments;

namespace EHive.Payments.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapReceipt();
            MapBankSlip();
            MapBankSlipSettings();
            MapPaymentCheckoutToPayment();
        }

        private void MapBankSlipSettings()
        {
            CreateMap<BankSlipSettingsDto, BankSlipSettings>()
              .ReverseMap();
        }

        private void MapBankSlip()
        {
            CreateMap<BankSlipInfoDto, BankSlipInfo>()
              .ReverseMap();

            CreateMap<BankSlipDataDto, BankSlipData>()
              .ReverseMap();
        }

        private void MapReceipt()
        {
            CreateMap<PaymentReceiptDto, PaymentReceipt>()
              .ReverseMap();
        }

        private void MapPaymentCheckoutToPayment()
        {
            CreateMap<PaymentCheckoutDto, Payment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PaymentId ?? Guid.NewGuid().ToString()))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId ?? string.Empty))
                .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
                .ForMember(dest => dest.ProviderKey, opt => opt.MapFrom(src => src.ProviderKey))
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Library.Enums.Payments.PaymentStatus.Pending))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.OriginalValue, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                .ForPath(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
                .ForPath(dest => dest.BankSlipInfo, opt => opt.MapFrom(src => src.BankSlipInfo));
        }
    }
}