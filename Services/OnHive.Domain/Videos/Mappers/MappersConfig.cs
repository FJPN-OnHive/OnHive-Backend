using AutoMapper;
using OnHive.Core.Library.Contracts.Videos;
using OnHive.Core.Library.Entities.Videos;

namespace OnHive.Videos.Domain.Mappers
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
