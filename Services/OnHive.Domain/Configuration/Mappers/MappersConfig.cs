using AutoMapper;
using EHive.Core.Library.Contracts.Configuration;
using EHive.Core.Library.Entities.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Text.Json;

namespace EHive.Configuration.Domain.Mappers
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