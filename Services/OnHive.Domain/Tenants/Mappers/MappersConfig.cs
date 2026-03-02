using AutoMapper;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Entities.Tenants;

namespace EHive.Tenants.Domain.Mappers
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
                .ReverseMap();
        }
    }
}