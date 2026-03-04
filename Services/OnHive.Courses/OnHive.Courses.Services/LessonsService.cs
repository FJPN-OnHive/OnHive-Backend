using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Courses.Domain.Models;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace OnHive.Courses.Services
{
    public class LessonsService : ILessonsService
    {
        private readonly ILessonsRepository lessonsRepository;
        private readonly IExamsService examsService;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public LessonsService(ILessonsRepository lessonsRepository,
                              CoursesApiSettings coursesApiSettings,
                              IMapper mapper,
                              IExamsService examsService)
        {
            this.lessonsRepository = lessonsRepository;
            this.coursesApiSettings = coursesApiSettings;
            this.examsService = examsService;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<LessonDto?> GetByIdAsync(string classId, UserDto? loggedUser)
        {
            var lesson = await lessonsRepository.GetByIdAsync(classId);
            var result = mapper.Map<LessonDto>(lesson);
            result = await GetExams(result, loggedUser);
            return result;
        }

        public async Task<LessonDto?> GetByIdAsync(string classId)
        {
            var lesson = await lessonsRepository.GetByIdAsync(classId);
            var result = mapper.Map<LessonDto>(lesson);
            return result;
        }

        public async Task<PaginatedResult<LessonDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto loggedUser)
        {
            var resultEntity = await lessonsRepository.GetByFilterAndIdsAsync(filter, lessonIds, loggedUser?.TenantId);
            var result = new PaginatedResult<LessonDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<LessonDto>()
            };
            if (resultEntity != null)
            {
                result = new PaginatedResult<LessonDto>
                {
                    Page = resultEntity.Page,
                    PageCount = resultEntity.PageCount,
                    Itens = mapper.Map<List<LessonDto>>(resultEntity.Itens)
                };
                foreach (var item in result.Itens)
                {
                    await GetExams(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<PaginatedResult<LessonDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var resultEntity = await lessonsRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            var result = new PaginatedResult<LessonDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<LessonDto>()
            };
            if (resultEntity != null)
            {
                result = new PaginatedResult<LessonDto>
                {
                    Page = resultEntity.Page,
                    PageCount = resultEntity.PageCount,
                    Itens = mapper.Map<List<LessonDto>>(resultEntity.Itens)
                };
                foreach (var item in result.Itens)
                {
                    await GetExams(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<IEnumerable<LessonDto>> GetAllAsync(UserDto? loggedUser)
        {
            var lessons = await lessonsRepository.GetAllAsync(loggedUser?.TenantId);
            var result = mapper.Map<IEnumerable<LessonDto>>(lessons);
            foreach (var item in result)
            {
                await GetExams(item, loggedUser);
            }
            return result;
        }

        public async Task<LessonDto> SaveAsync(LessonDto lessonDto, UserDto? loggedUser)
        {
            var lesson = mapper.Map<Lesson>(lessonDto);
            ValidatePermissions(lesson, loggedUser);
            lesson.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            lesson.CreatedAt = DateTime.UtcNow;
            lesson.CreatedBy = string.IsNullOrEmpty(lesson.CreatedBy) ? loggedUser.Id : lesson.CreatedBy;
            lesson.Exam = mapper.Map<Exam>(await SaveExam(lessonDto.Exam, loggedUser));
            lesson.IsActive = true;
            var response = await lessonsRepository.SaveAsync(lesson);
            return mapper.Map<LessonDto>(response);
        }

        private async Task<ExamDto?> SaveExam(ExamDto? exam, UserDto loggedUser)
        {
            if (exam == null)
            {
                return null;
            }
            var savedExam = await examsService.SaveAsync(exam, loggedUser);
            if (savedExam == null)
            {
                return null;
            }

            return savedExam;
        }

        public async Task<LessonDto> CreateAsync(LessonDto lessonDto, UserDto loggedUser)
        {
            var lesson = mapper.Map<Lesson>(lessonDto);
            ValidatePermissions(lesson, loggedUser);
            lesson.Id = string.Empty;
            lesson.TenantId = loggedUser.TenantId;
            lesson.Exam = mapper.Map<Exam>(await SaveExam(lessonDto.Exam, loggedUser));
            lesson.IsActive = true;
            var response = await lessonsRepository.SaveAsync(lesson, loggedUser.Id);
            return mapper.Map<LessonDto>(response);
        }

        public async Task<LessonDto?> UpdateAsync(LessonDto lessonDto, UserDto loggedUser)
        {
            var lesson = mapper.Map<Lesson>(lessonDto);
            ValidatePermissions(lesson, loggedUser);
            var currentClass = await lessonsRepository.GetByIdAsync(lesson.Id);
            if (currentClass == null || currentClass.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            lesson.Exam = mapper.Map<Exam>(await SaveExam(lessonDto.Exam, loggedUser));
            lesson.IsActive = true;
            var response = await lessonsRepository.SaveAsync(lesson, loggedUser.Id);
            return mapper.Map<LessonDto>(response);
        }

        public async Task<LessonDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentLesson = await lessonsRepository.GetByIdAsync(patch.GetId());
            if (currentLesson == null || currentLesson.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentLesson = patch.PatchEntity(currentLesson);
            if (!mapper.Map<LessonDto>(currentLesson).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentLesson, loggedUser);
            currentLesson.IsActive = true;
            var response = await lessonsRepository.SaveAsync(currentLesson, loggedUser.Id);
            return mapper.Map<LessonDto>(response);
        }

        private async Task<LessonDto> GetExams(LessonDto result, UserDto? loggedUser)
        {
            if (result.Exam != null)
            {
                result.Exam = await examsService.GetByIdAsync(result.Exam.Id, loggedUser);
            }
            return result;
        }

        private void ValidatePermissions(Lesson @class, UserDto? loggedUser)
        {
            if (loggedUser != null && @class.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Class/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                                @class.Id, @class.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}