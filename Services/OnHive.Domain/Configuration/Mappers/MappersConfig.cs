using AutoMapper;
using OnHive.Core.Library.Contracts.Configuration;
using OnHive.Core.Library.Entities.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Text.Json;

namespace OnHive.Configuration.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapCinfigItemToCnfigItemDto();
        }

        private void MapCinfigItemToCnfigItemDto()
        {
            CreateMap<ConfigItem, ConfigItemDto>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => JsonSerializer.Deserialize<object>(src.Value)))
                .ReverseMap()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => JsonSerializer.Serialize(src)));
        }
    }
}