using AutoMapper;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Entities.Courses;

namespace EHive.Courses.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapDisciplineToDisciplineDto();
            MapCourseToCourseDto();
            MapCourseToCourseResumeDto();
            MapClassToClassDto();
            MapExamToExamDto();
        }

        private void MapExamToExamDto()
        {
            CreateMap<ExamQuestion, ExamQuestionDto>()
                .ReverseMap();

            CreateMap<QuestionOption, QuestionOptionDto>()
                .ReverseMap();

            CreateMap<Exam, ExamDto>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.VId) ? src.VId : src.Id))
                 .ForMember(dest => dest.VId, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.VId) ? src.VId : src.Id))
                .ReverseMap()
                 .ForMember(dest => dest.VId, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.VId) ? src.VId : src.Id));
        }

        private void MapCourseToCourseDto()
        {
            CreateMap<CourseStaff, CourseStaffDto>()
                .ReverseMap();

            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.TotalTimeMinutes, opt => opt.MapFrom(src => src.Duration * 60))
                .ForPath(dest => dest.Disciplines, opt => opt.MapFrom(src => src.Disciplines.Select(d => new DisciplineDto { Id = d })))
                .ReverseMap()
                .ForPath(dest => dest.Disciplines, opt => opt.MapFrom(src => src.Disciplines.Select(c => c.Id)));
        }

        private void MapCourseToCourseResumeDto()
        {
            CreateMap<DisciplineDto, DisciplineResumeDto>()
                .ReverseMap();

            CreateMap<Discipline, DisciplineResumeDto>()
                .ForPath(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.Select(c => new LessonResumeDto { Id = c })))
                .ReverseMap()
                .ForPath(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.Select(c => c.Id)));

            CreateMap<Lesson, LessonResumeDto>()
                .ReverseMap();

            CreateMap<Course, CourseResumeDto>()
                .ForPath(dest => dest.Disciplines, opt => opt.MapFrom(src => src.Disciplines.Select(c => new DisciplineResumeDto { Id = c })))
                .ReverseMap()
                .ForPath(dest => dest.Disciplines, opt => opt.MapFrom(src => src.Disciplines.Select(c => c.Id)));

            CreateMap<ProductDto, CourseProductDto>()
                .ReverseMap();
        }

        private void MapClassToClassDto()
        {
            CreateMap<Material, MaterialDto>()
                .ReverseMap();

            CreateMap<LessonDto, LessonResumeDto>()
                .ReverseMap();

            CreateMap<Lesson, LessonResumeDto>()
                .ReverseMap();

            CreateMap<Lesson, LessonDto>()
                .ReverseMap();
        }

        private void MapDisciplineToDisciplineDto()
        {
            CreateMap<Discipline, DisciplineDto>()
                .ForPath(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.Select(e => new LessonDto { Id = e })))
                .ForPath(dest => dest.Exams, opt => opt.MapFrom(src => src.Exams.Select(e => new ExamDto { Id = e })))
                .ReverseMap()
                .ForPath(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.Select(e => e.Id)))
                .ForPath(dest => dest.Exams, opt => opt.MapFrom(src => src.Exams.Select(e => e.Id)));
        }
    }
}