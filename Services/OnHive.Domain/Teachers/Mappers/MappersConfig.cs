using AutoMapper;
using OnHive.Core.Library.Contracts.Teachers;
using OnHive.Core.Library.Entities.Teachers;

namespace OnHive.Teachers.Domain.Mappers
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
