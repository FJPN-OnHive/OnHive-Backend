using AutoMapper;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Entities.Redirects;

namespace OnHive.Redirects.Domain.Mappers
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