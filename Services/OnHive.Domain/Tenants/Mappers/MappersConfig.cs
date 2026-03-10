using AutoMapper;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Entities.Tenants;

namespace OnHive.Tenants.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapTenantToTenantDto();
            MapTenantParameterToTenantParameterDto();
            MapFeaturesToFeatureDto();
            MapTenantThemeToTenantThemeDto();
            MapTenantToTenantResumeDto();
        }

        private void MapTenantToTenantDto()
        {
            CreateMap<Tenant, TenantDto>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features.Select(f => f.Key)))
                .ReverseMap()
                .ForMember(dest => dest.Features, opt => opt.Ignore());
        }

        private void MapTenantParameterToTenantParameterDto()
        {
            CreateMap<TenantParameter, TenantParameterDto>()
                .ReverseMap();
        }

        private void MapTenantThemeToTenantThemeDto()
        {
            CreateMap<ColorToken, ColorTokenDto>()
                .ReverseMap();

            CreateMap<StatusToken, StatusTokenDto>()
                .ReverseMap();

            CreateMap<FontToken, FontTokenDto>()
                .ReverseMap();

            CreateMap<FontFamilyToken, FontFamilyTokenDto>()
                .ReverseMap();

            CreateMap<TenantTokens, TenantTokensDto>()
                .ReverseMap();
            
            CreateMap<TenantTheme, TenantThemeDto>()
                .ReverseMap();
        }

        private void MapFeaturesToFeatureDto()
        {
            CreateMap<Feature, FeatureDto>()
                .ReverseMap();
        }

        private void MapTenantToTenantResumeDto()
        {
            CreateMap<Tenant, TenantResumeDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address}, {src.District} - {src.City} - {src.State}"))
                .ReverseMap();
        }
    }
}