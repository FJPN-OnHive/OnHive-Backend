using AutoMapper;
using EHive.Core.Library.Contracts.Posts;
using EHive.Core.Library.Entities.Posts;

namespace EHive.Posts.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapPostMetaDataToPostMetaDataDto();
            MapBlogToBlogPostDto();
            MapPostBackup();
        }

        private void MapBlogToBlogPostDto()
        {
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => src.Likes.Count))
                .ReverseMap()
                .ForMember(dest => dest.Likes, opt => opt.Ignore());
        }

        private void MapPostMetaDataToPostMetaDataDto()
        {
            CreateMap<PostMetadata, PostMetadataDto>()
                .ReverseMap();
        }

        private void MapPostBackup()
        {
            CreateMap<BlogPost, BlogPostBackup>()
                .ReverseMap();
        }
    }
}