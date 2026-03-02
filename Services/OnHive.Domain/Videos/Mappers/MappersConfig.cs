using AutoMapper;
using EHive.Core.Library.Contracts.Videos;
using EHive.Core.Library.Entities.Videos;

namespace EHive.Videos.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapVideoToVideoDto();
        }

        private void MapVideoToVideoDto()
        {
            CreateMap<Video, VideoDto>()
                .ReverseMap();
        }
    }
}
