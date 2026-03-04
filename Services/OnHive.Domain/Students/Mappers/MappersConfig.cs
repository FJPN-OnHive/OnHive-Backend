using AutoMapper;
using OnHive.Core.Library.Constants;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Entities.Users;

namespace OnHive.Students.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapUserToUserDto();
            MapStudentUsersToStudentUsersDto();
            MapStudentToStudentDto();
            MapStudentToStudentResumeDto();
            MapStudentActivityToStudentActivityDto();
            MapStudentReportToStudentReportDto();
        }

        private void MapStudentToStudentDto()
        {
            CreateMap<StudentExamQuestion, StudentExamQuestionDto>()
                .ReverseMap();

            CreateMap<StudentExam, StudentExamDto>()
                .ReverseMap();

            CreateMap<StudentDiscipline, StudentDisciplineDto>()
                .ReverseMap();

            CreateMap<StudentCourse, StudentCourseDto>()
                .ReverseMap();

            CreateMap<StudentLesson, StudentLessonsDto>()
                .ReverseMap();

            CreateMap<StudentAnnotation, StudentAnnotationDto>()
                .ReverseMap();

            CreateMap<Student, StudentDto>()
                .ReverseMap();
        }

        private void MapStudentToStudentResumeDto()
        {
            CreateMap<StudentCourse, StudentCourseResumeDto>()
                .ReverseMap();

            CreateMap<StudentDiscipline, StudentDisciplineResumeDto>()
                .ReverseMap();

            CreateMap<StudentLesson, StudentLessonsResumeDto>()
                .ReverseMap();

            CreateMap<StudentCourseDto, StudentCourseResumeDto>()
                .ReverseMap();

            CreateMap<StudentDisciplineDto, StudentDisciplineResumeDto>()
                    .ReverseMap();

            CreateMap<StudentLessonsDto, StudentLessonsResumeDto>()
                .ReverseMap();
        }

        private void MapStudentUsersToStudentUsersDto()
        {
            CreateMap<StudentUser, StudentUserDto>()
                .ForPath(dest => dest.Student, opt => opt.MapFrom(src => src))
                .ForPath(dest => dest.User, opt => opt.MapFrom(src => src.User));
        }

        private void MapStudentActivityToStudentActivityDto()
        {
            CreateMap<StudentActivity, StudentActivityDto>()
                .ReverseMap();
        }

        private void MapStudentReportToStudentReportDto()
        {
            CreateMap<StudentReport, StudentReportDto>()
                .ReverseMap();
        }

        private void MapUserToUserDto()
        {
            CreateMap<User, UserDto>()
                .ReverseMap();

            CreateMap<UserDocument, UserDocumentDto>()
                .ReverseMap();

            CreateMap<UserEmail, UserEmailDto>()
                .ReverseMap();

            CreateMap<Address, AddressDto>()
               .ForPath(dest => dest.State, opt => opt.MapFrom(src => new StateDto { Code = src.State, Name = AddressConstants.GetStateName(src.State) }))
               .ForPath(dest => dest.Country, opt => opt.MapFrom(src => new CountryDto { Code = src.Country, Name = AddressConstants.GetCountryName(src.Country) }))
               .ReverseMap()
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.Code))
               .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Code));
        }
    }
}