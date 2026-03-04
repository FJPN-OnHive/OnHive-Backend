using AutoMapper;
using OnHive.Core.Library.Contracts.SystemParameters;
using OnHive.Core.Library.Entities.SystemParameters;

namespace OnHive.SystemParameters.Domain.Mappers
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