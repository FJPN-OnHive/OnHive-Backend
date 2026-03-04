using AutoMapper;
using OnHive.Core.Library.Contracts.Dict;
using OnHive.Core.Library.Entities.Dict;

namespace OnHive.Dict.Domain.Mappers
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
