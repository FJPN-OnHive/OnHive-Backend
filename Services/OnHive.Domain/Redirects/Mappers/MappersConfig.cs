using AutoMapper;
using EHive.Core.Library.Contracts.Redirects;
using EHive.Core.Library.Entities.Redirects;

namespace EHive.Redirects.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapRedirectToRedirectDto();
        }

        private void MapRedirectToRedirectDto()
        {
            CreateMap<Redirect, RedirectDto>()
                .ReverseMap();
        }
    }
}