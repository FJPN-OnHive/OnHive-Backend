using AutoMapper;
using EHive.Core.Library.Contracts.SystemParameters;
using EHive.Core.Library.Entities.SystemParameters;

namespace EHive.SystemParameters.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapSystemParameterToSystemParameterDto();
        }

        private void MapSystemParameterToSystemParameterDto()
        {
            CreateMap<SystemParameter, SystemParameterDto>()
                .ReverseMap();
        }
    }
}