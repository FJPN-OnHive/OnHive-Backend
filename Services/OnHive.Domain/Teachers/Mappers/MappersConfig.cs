using AutoMapper;
using EHive.Core.Library.Contracts.Teachers;
using EHive.Core.Library.Entities.Teachers;

namespace EHive.Teachers.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapTeacherToTeacherDto();
        }

        private void MapTeacherToTeacherDto()
        {
            CreateMap<Teacher, TeacherDto>()
                .ReverseMap();
        }
    }
}
