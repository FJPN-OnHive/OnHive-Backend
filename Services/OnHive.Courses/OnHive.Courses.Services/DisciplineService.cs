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
using System.Text.Json;

namespace OnHive.Courses.Services
{
    public class DisciplineService : IDisciplineService
    {
        private readonly IDisciplineRepository disciplineRepository;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly ILessonsService lessonsService;
        private readonly IExamsService examsService;

        public DisciplineService(IDisciplineRepository disciplineRepository,
                                 CoursesApiSettings coursesApiSettings,
                                 IMapper mapper,
                                 ILessonsService lessonsService,
                                 IExamsService examsService)
        {
            this.disciplineRepository = disciplineRepository;
            this.coursesApiSettings = coursesApiSettings;
            this.mapper = mapper;
            this.lessonsService = lessonsService;
            this.examsService = examsService;
            logger = Log.Logger;
        }

        public async Task<DisciplineDto?> GetByIdAsync(string disciplineId, UserDto? loggedUser)
        {
            var discipline = await disciplineRepository.GetByIdAsync(disciplineId);
            var result = mapper.Map<DisciplineDto>(discipline);
            result = await GetLessons(result, loggedUser);
            result = await GetExams(result, loggedUser);
            return result;
        }

        public async Task<DisciplineDto?> GetByIdAsync(string disciplineId)
        {
            var discipline = await disciplineRepository.GetByIdAsync(disciplineId);
            var result = mapper.Map<DisciplineDto>(discipline);
            result = await GetLessons(result);
            return result;
        }

        public async Task<PaginatedResult<DisciplineDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var entityResult = await disciplineRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            var result = new PaginatedResult<DisciplineDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<DisciplineDto>()
            };
            if (entityResult != null)
            {
                result = new PaginatedResult<DisciplineDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<DisciplineDto>>(entityResult.Itens)
                };

                foreach (var item in result.Itens)
                {
                    await GetLessons(item, loggedUser);
                    await GetExams(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<PaginatedResult<DisciplineDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto? loggedUser)
        {
            var entityResult = await disciplineRepository.GetByFilterAndIdsAsync(filter, lessonIds, loggedUser?.TenantId);
            var result = new PaginatedResult<DisciplineDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<DisciplineDto>()
            };
            if (entityResult != null)
            {
                result = new PaginatedResult<DisciplineDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<DisciplineDto>>(entityResult.Itens)
                };

                foreach (var item in result.Itens)
                {
                    await GetLessons(item, loggedUser);
                    await GetExams(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<IEnumerable<DisciplineDto>> GetAllAsync(UserDto? loggedUser)
        {
            var disciplines = await disciplineRepository.GetAllAsync(loggedUser?.TenantId);
            var result = mapper.Map<IEnumerable<DisciplineDto>>(disciplines);
            foreach (var item in result)
            {
                await GetLessons(item, loggedUser);
                await GetExams(item, loggedUser);
            }
            return result;
        }

        public async Task<DisciplineDto> SaveAsync(DisciplineDto disciplineDto, UserDto? loggedUser)
        {
            if (string.IsNullOrEmpty(disciplineDto.Code))
            {
                disciplineDto.Code = CodeHelper.GenerateNumericCode(11);
            }
            foreach (var item in disciplineDto.Lessons.Where(d => string.IsNullOrEmpty(d.Code)))
            {
                item.Code = $"{disciplineDto.Code}-{disciplineDto.Lessons.IndexOf(item) + 1}";
            }
            var discipline = mapper.Map<Discipline>(disciplineDto);
            ValidatePermissions(discipline, loggedUser);
            discipline.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            discipline.CreatedAt = DateTime.UtcNow;
            discipline.IsActive = true;
            discipline.CreatedBy = string.IsNullOrEmpty(discipline.CreatedBy) ? loggedUser.Id : discipline.CreatedBy;
            discipline.Lessons = await SaveLessons(disciplineDto.Lessons, loggedUser);
            discipline.Exams = await SaveExams(disciplineDto.Exams, loggedUser);
            var response = await disciplineRepository.SaveAsync(discipline);
            return mapper.Map<DisciplineDto>(response);
        }

        private async Task<List<string>> SaveExams(List<ExamDto> exams, UserDto loggedUser)
        {
            var result = new List<string>();
            foreach (var item in exams)
            {
                var exam = await examsService.SaveAsync(item, loggedUser);
                if (exam != null)
                {
                    result.Add(exam.Id);
                }
            }
            return result;
        }

        private async Task<List<string>> SaveLessons(List<LessonDto> lessons, UserDto loggedUser)
        {
            var result = new List<string>();
            foreach (var item in lessons)
            {
                var lesson = await lessonsService.SaveAsync(item, loggedUser);
                if (lesson != null)
                {
                    result.Add(lesson.Id);
                }
            }
            return result;
        }

        public async Task<DisciplineDto> CreateAsync(DisciplineDto disciplineDto, UserDto loggedUser)
        {
            if (!disciplineDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var discipline = mapper.Map<Discipline>(disciplineDto);
            ValidatePermissions(discipline, loggedUser);
            discipline.Id = string.Empty;
            discipline.TenantId = loggedUser.TenantId;
            discipline.IsActive = true;
            discipline.Lessons = await SaveLessons(disciplineDto.Lessons, loggedUser);
            discipline.Exams = await SaveExams(disciplineDto.Exams, loggedUser);
            var response = await disciplineRepository.SaveAsync(discipline, loggedUser.Id);
            return mapper.Map<DisciplineDto>(response);
        }

        public async Task<DisciplineDto?> UpdateAsync(DisciplineDto disciplineDto, UserDto loggedUser)
        {
            if (!disciplineDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var discipline = mapper.Map<Discipline>(disciplineDto);
            discipline.Lessons = await SaveLessons(disciplineDto.Lessons, loggedUser);
            discipline.Exams = await SaveExams(disciplineDto.Exams, loggedUser);
            discipline.IsActive = true;
            ValidatePermissions(discipline, loggedUser);
            var currentDiscipline = await disciplineRepository.GetByIdAsync(discipline.Id);
            if (currentDiscipline == null || currentDiscipline.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            var response = await disciplineRepository.SaveAsync(discipline, loggedUser.Id);
            return mapper.Map<DisciplineDto>(response);
        }

        public async Task<DisciplineDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentDiscipline = await disciplineRepository.GetByIdAsync(patch.GetId());
            if (currentDiscipline == null || currentDiscipline.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentDiscipline = patch.PatchEntity(currentDiscipline);
            if (!mapper.Map<DisciplineDto>(currentDiscipline).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentDiscipline, loggedUser);
            currentDiscipline.IsActive = true;
            var response = await disciplineRepository.SaveAsync(currentDiscipline, loggedUser.Id);
            return mapper.Map<DisciplineDto>(response);
        }

        private async Task<DisciplineDto> GetExams(DisciplineDto result, UserDto? loggedUser)
        {
            if (result != null && result.Exams.Any())
            {
                var exams = new List<ExamDto>();
                foreach (var examItem in result.Exams)
                {
                    exams.Add(await examsService.GetByIdAsync(examItem.Id, loggedUser));
                }
                result.Exams = exams;
            }
            return result;
        }

        private async Task<DisciplineDto> GetLessons(DisciplineDto result, UserDto? loggedUser)
        {
            if (result != null && result.Lessons.Any())
            {
                var lessons = new List<LessonDto>();
                foreach (var classItem in result.Lessons)
                {
                    lessons.Add(await lessonsService.GetByIdAsync(classItem.Id, loggedUser));
                }
                result.Lessons = lessons;
            }
            return result;
        }

        private async Task<DisciplineDto> GetLessons(DisciplineDto result)
        {
            if (result != null && result.Lessons.Any())
            {
                var lessons = new List<LessonDto>();
                foreach (var classItem in result.Lessons)
                {
                    lessons.Add(await lessonsService.GetByIdAsync(classItem.Id));
                }
                result.Lessons = lessons;
            }
            return result;
        }

        private void ValidatePermissions(Discipline discipline, UserDto? loggedUser)
        {
            if (loggedUser != null && discipline.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Discipline/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    discipline.Id, discipline.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}