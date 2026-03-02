using AutoMapper;
using EHive.Core.Library.Contracts.Dict;
using EHive.Core.Library.Entities.Dict;

namespace EHive.Dict.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapValuesToValuesDto();
        }

        private void MapValuesToValuesDto()
        {
            CreateMap<ValueRegistry, ValueRegistryDto>()
                .ReverseMap();
        }
    }
}
