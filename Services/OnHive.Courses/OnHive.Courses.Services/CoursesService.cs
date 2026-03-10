using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Enums.Common;
using OnHive.Core.Library.Extensions;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Courses.Domain.Models;
using OnHive.Enrich.Library.Extensions;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Repositories;
using Serilog;
using System.Text;
using System.Text.Json;

namespace OnHive.Courses.Services
{
    public class CoursesService : ICoursesService
    {
        private readonly ICoursesRepository coursesRepository;
        private readonly IDisciplineService disciplineService;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IStudentsRepository studentsRepository;
        private readonly IProductsService productsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEventRegister eventRegister;

        public CoursesService(ICoursesRepository coursesRepository,
                              IDisciplineService disciplineService,
                              CoursesApiSettings coursesApiSettings,
                              IMapper mapper,
                              IEventRegister eventRegister,
                              IStudentsRepository studentsRepository,
                              IProductsService productsService)
        {
            this.coursesRepository = coursesRepository;
            this.disciplineService = disciplineService;
            this.coursesApiSettings = coursesApiSettings;
            this.studentsRepository = studentsRepository;
            this.productsService = productsService;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<CourseDto?> GetByIdAsync(string courseId)
        {
            var course = await coursesRepository.GetByIdAsync(courseId);
            if (course != null && !course.IsActive) return null;
            var result = mapper.Map<CourseDto>(course);
            await result.LoadEnrichmentAsync();
            result = await GetDisciplinesResume(result);
            return result;
        }

        public async Task<CourseDto?> GetByIdInternalAsync(string courseId)
        {
            var course = await coursesRepository.GetByIdAsync(courseId);
            var result = mapper.Map<CourseDto>(course);
            await result.LoadEnrichmentAsync();
            result = await GetDisciplines(result, new UserDto { Permissions = [coursesApiSettings.CoursesAdminPermission] });
            return result;
        }

        public async Task<List<CourseDto>> GetAllByTenantInternalAsync(string tenantId)
        {
            var course = await coursesRepository.GetAllAsync(tenantId);
            return mapper.Map<List<CourseDto>>(course);
        }

        public async Task<CourseDto?> GetByIdAsync(string courseId, UserDto? loggedUser)
        {
            var course = await coursesRepository.GetByIdAsync(courseId);
            var result = mapper.Map<CourseDto>(course);
            await result.LoadEnrichmentAsync();
            result = await GetDisciplines(result, loggedUser);
            return result;
        }

        public async Task<PaginatedResult<CourseDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var entityResult = await coursesRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            var result = new PaginatedResult<CourseDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CourseDto>()
            };
            if (entityResult != null)
            {
                result = new PaginatedResult<CourseDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<CourseDto>>(entityResult.Itens)
                };
                foreach (var item in result.Itens)
                {
                    await item.LoadEnrichmentAsync();
                    await GetDisciplines(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<PaginatedResult<CourseDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto loggedUser)
        {
            var entityResult = await coursesRepository.GetByFilterAndIdsAsync(filter, lessonIds, loggedUser?.TenantId);
            var result = new PaginatedResult<CourseDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CourseDto>()
            };
            if (entityResult != null)
            {
                result = new PaginatedResult<CourseDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<CourseDto>>(entityResult.Itens)
                };
                foreach (var item in result.Itens)
                {
                    await item.LoadEnrichmentAsync();
                    await GetDisciplines(item, loggedUser);
                }
            }
            return result;
        }

        public async Task<PaginatedResult<CourseResumeDto>> GetByFilterByUserAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = new PaginatedResult<CourseResumeDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CourseResumeDto>()
            };
            var courses = await GetUserCourses(loggedUser.Id);
            if (courses.Count == 0) return result;

            var entityResult = await coursesRepository.GetByFilterAndUserAsync(courses, filter, loggedUser?.TenantId);

            if (entityResult != null)
            {
                result = new PaginatedResult<CourseResumeDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<CourseResumeDto>>(entityResult.Itens)
                };
            }
            return result;
        }

        public async Task<PaginatedResult<CourseResumeDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId)
        {
            var entityResult = await coursesRepository.GetByFilterAsync(filter, tenantId);
            var result = new PaginatedResult<CourseResumeDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CourseResumeDto>()
            };
            if (entityResult != null)
            {
                result = new PaginatedResult<CourseResumeDto>
                {
                    Page = entityResult.Page,
                    PageCount = entityResult.PageCount,
                    Itens = mapper.Map<List<CourseResumeDto>>(entityResult.Itens)
                };
                foreach (var item in result.Itens)
                {
                    await GetDisciplines(item);
                }
                result = await GetProducts(result);
            }
            return result;
        }

        public async Task<Stream> GetExportData(ExportFormats format, string tenantId, bool activeOnly)
        {
            var courses = activeOnly
                ? await coursesRepository.GetAllActive(tenantId)
                : await coursesRepository.GetAllAsync(tenantId);

            var stream = format switch
            {
                ExportFormats.Csv => ToCsvStream(courses),
                ExportFormats.Json => ToJsonStream(courses),
                ExportFormats.Xml => ToXmlStream(courses),
                _ => throw new NotImplementedException()
            };
            return stream;
        }

        private async Task<List<string>> GetUserCourses(string userId)
        {
            var student = await studentsRepository.GetByUserIdAsync(userId);
            if (student == null || student.Courses == null || student.Courses.Count == 0) return [];
            return student.Courses.Where(c => c.IsActive).Select(c => c.Id).ToList();
        }

        private Stream ToXmlStream(List<Course> courses)
        {
            var resultXml = $@"<?xml version=""1.0""?>
                               <courses>
                                   {GetXmlItens(courses)}
                               </courses>";
            return new MemoryStream(Encoding.UTF8.GetBytes(resultXml));
        }

        private string GetXmlItens(List<Course> courses)
        {
            var result = "";
            foreach (var course in courses)
            {
                var active = course.IsActive ? "yes" : "no";
                var launchDate = course.LaunchDate.HasValue ? course.LaunchDate.Value.ToString("s") : "";
                result += @$"<course>
                                <code>{course.Code.EscapeXml()}</code>
                                <name>{course.Name.EscapeXml()}</name>
                                <description>{course.Description.EscapeXml()}</description>
                                <slug>{course.Slug.EscapeXml()}</slug>
                                <imageUrl>{course.ImageUrl.EscapeXml()}</imageUrl>
                                <launchDate>{launchDate}</launchDate>
                                <tenantId>{course.TenantId}</tenantId>
                                <active>{active}</active>
                            </course>
                            ";
            }
            return result;
        }

        private Stream ToJsonStream(List<Course> courses)
        {
            var result = JsonSerializer.Serialize(courses);
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream ToCsvStream(List<Course> courses)
        {
            var result = $"id;code;name;description;Image Url;slug;launchDate;tenantId;active\n";
            foreach (var course in courses)
            {
                var active = course.IsActive ? "yes" : "no";
                var launchDate = course.LaunchDate.HasValue ? course.LaunchDate.Value.ToString("s") : "";
                result += $"{course.Id.Replace(";", " ")};{course.Code.Replace(";", " ")};{course.Name.Replace(";", " ")};{course.Description.Replace(";", " ")};{course.ImageUrl.Replace(";", " ")};{course.Slug.Replace(";", " ")};{launchDate};{course.TenantId.Replace(";", " ")};{active}\n";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private async Task<PaginatedResult<CourseResumeDto>> GetProducts(PaginatedResult<CourseResumeDto> result)
        {
            var itensIds = result.Itens.Select(c => c.Id).Distinct().ToList();
            var products = await productsService.GetByItemIdsAsync(itensIds);
            foreach (var item in result.Itens)
            {
                var productFull = products.FirstOrDefault(p => p.ItemId == item.Id);
                if (productFull != null)
                {
                    item.Product = mapper.Map<CourseProductDto>(productFull);
                }
            }
            return result;
        }

        public async Task<IEnumerable<CourseDto>> GetAllAsync(UserDto? loggedUser)
        {
            var courses = await coursesRepository.GetAllAsync(loggedUser?.TenantId);
            var result = mapper.Map<IEnumerable<CourseDto>>(courses);
            foreach (var item in result)
            {
                await item.LoadEnrichmentAsync();
                await GetDisciplines(item, loggedUser);
            }
            return result;
        }

        public async Task<CourseDto> SaveAsync(CourseDto courseDto, UserDto? loggedUser)
        {
            if (!courseDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            if (string.IsNullOrEmpty(courseDto.Code))
            {
                courseDto.Code = CodeHelper.GenerateNumericCode(11);
            }
            foreach (var item in courseDto.Disciplines.Where(d => string.IsNullOrEmpty(d.Code)))
            {
                item.Code = $"{courseDto.Code}-{courseDto.Disciplines.IndexOf(item) + 1}";
            }
            if (string.IsNullOrEmpty(courseDto.Slug))
            {
                courseDto.Slug = courseDto.Name.NormalizeSlug();
            }
            var course = mapper.Map<Course>(courseDto);
            ValidatePermissions(course, loggedUser);
            course.TenantId = loggedUser?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            course.CreatedAt = DateTime.UtcNow;
            course.CreatedBy = string.IsNullOrEmpty(course.CreatedBy) ? loggedUser.Id : course.CreatedBy;
            course.Disciplines = await SaveDisciplines(courseDto.Disciplines, loggedUser);
            var response = await coursesRepository.SaveAsync(course);
            await courseDto.SaveEnrichmentAsync();
            var result = mapper.Map<CourseDto>(response);
            result.CustomAttributes = courseDto.CustomAttributes;
            RegisterEvent(EventKeys.CourseCreated, "Course created", loggedUser.TenantId, loggedUser.Id, response);
            logger.Information("Course created {Id}, by {User} Tenant {Tenant}", response.Id, loggedUser.Id, loggedUser.Tenant);
            return result;
        }

        public async Task<CourseDto> CreateAsync(CourseDto courseDto, UserDto loggedUser)
        {
            if (!courseDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            if (string.IsNullOrEmpty(courseDto.Code))
            {
                courseDto.Code = CodeHelper.GenerateNumericCode(11);
            }
            foreach (var item in courseDto.Disciplines.Where(d => string.IsNullOrEmpty(d.Code)))
            {
                item.Code = $"{courseDto.Code}-{courseDto.Disciplines.IndexOf(item) + 1}";
            }
            var course = mapper.Map<Course>(courseDto);
            ValidatePermissions(course, loggedUser);
            course.Id = string.Empty;
            course.TenantId = loggedUser.TenantId;
            course.IsActive = false;
            course.Disciplines = await SaveDisciplines(courseDto.Disciplines, loggedUser);
            var response = await coursesRepository.SaveAsync(course, loggedUser.Id);
            await courseDto.SaveEnrichmentAsync();
            var result = mapper.Map<CourseDto>(response);
            result.CustomAttributes = courseDto.CustomAttributes;
            RegisterEvent(EventKeys.CourseCreated, "Course created", loggedUser.TenantId, loggedUser.Id, response);
            logger.Information("Course created {Id}, by {User} Tenant {Tenant}", response.Id, loggedUser.Id, loggedUser.Tenant);
            return result;
        }

        private async Task<List<string>> SaveDisciplines(List<DisciplineDto> disciplines, UserDto loggedUser)
        {
            var result = new List<string>();

            foreach (var item in disciplines)
            {
                var discipline = await disciplineService.SaveAsync(item, loggedUser);
                if (discipline != null)
                {
                    result.Add(discipline.Id);
                }
            }

            return result;
        }

        public async Task<CourseDto?> UpdateAsync(CourseDto courseDto, UserDto loggedUser)
        {
            if (!courseDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var course = mapper.Map<Course>(courseDto);
            ValidatePermissions(course, loggedUser);
            var currentCourse = await coursesRepository.GetByIdAsync(course.Id);
            if (currentCourse == null || currentCourse.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            course.Disciplines = await SaveDisciplines(courseDto.Disciplines, loggedUser);
            var response = await coursesRepository.SaveAsync(course, loggedUser.Id);
            RegisterEvent(EventKeys.CourseUpdated, "Course updated", loggedUser.TenantId, loggedUser.Id, response, currentCourse);
            logger.Information("Course updated {Id}, by {User} Tenant {Tenant}", response.Id, loggedUser.Id, loggedUser.Tenant);
            return mapper.Map<CourseDto>(response);
        }

        public async Task<CourseDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentCourse = await coursesRepository.GetByIdAsync(patch.GetId());
            if (currentCourse == null || currentCourse.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentCourse = patch.PatchEntity(currentCourse);
            if (!mapper.Map<CourseDto>(currentCourse).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentCourse, loggedUser);
            var response = await coursesRepository.SaveAsync(currentCourse, loggedUser.Id);
            RegisterEvent(EventKeys.CourseUpdated, "Course updated", loggedUser.TenantId, loggedUser.Id, response, currentCourse);
            logger.Information("Course updated {Id}, by {User} Tenant {Tenant}", response.Id, loggedUser.Id, loggedUser.Tenant);
            return mapper.Map<CourseDto>(response);
        }

        private async Task<CourseDto> GetDisciplines(CourseDto result, UserDto? loggedUser)
        {
            if (result != null && result.Disciplines.Any())
            {
                var disciplines = new List<DisciplineDto>();
                foreach (var disciplineItem in result.Disciplines)
                {
                    disciplines.Add(await disciplineService.GetByIdAsync(disciplineItem.Id, loggedUser));
                }
                result.Disciplines = disciplines;
            }
            return result;
        }

        private async Task<CourseDto> GetDisciplinesResume(CourseDto result)
        {
            if (result.Disciplines.Any())
            {
                var disciplines = new List<DisciplineDto>();
                foreach (var disciplineItem in result.Disciplines)
                {
                    var discipline = await disciplineService.GetByIdAsync(disciplineItem.Id);
                    var disciplineResume = mapper.Map<DisciplineResumeDto>(discipline);
                    discipline = mapper.Map<DisciplineDto>(disciplineResume);
                    disciplines.Add(discipline);
                }
                result.Disciplines = disciplines;
            }
            return result;
        }

        private async Task<CourseResumeDto> GetDisciplines(CourseResumeDto result)
        {
            if (result.Disciplines.Any())
            {
                var disciplines = new List<DisciplineResumeDto>();
                foreach (var disciplineItem in result.Disciplines)
                {
                    var fullDiscipline = await disciplineService.GetByIdAsync(disciplineItem.Id);
                    disciplines.Add(mapper.Map<DisciplineResumeDto>(fullDiscipline));
                }
                result.Disciplines = disciplines;
            }
            return result;
        }

        private void RegisterEvent(string key, string message, string tenantId, string UserId, Course newCourse, Course? oldCourse = null)
        {
            try
            {
                var newValues = JsonSerializer.Serialize(newCourse);
                var oldValues = string.Empty;
                if (oldCourse != null)
                {
                    oldValues = JsonSerializer.Serialize(oldCourse);
                }

                _ = eventRegister.RegisterEvent(tenantId, UserId, key, message,
                                                ["CourseId", "CourseName", "CurrentValues", "PreviousValues"],
                                                newCourse.Id, newCourse.Name, newValues, oldValues);
            }
            catch (Exception ex)
            {
                logger.Error("Fail registering event {key} for course {course}: -- error: {message}", key, newCourse.Id, ex.Message, ex);
            }
        }

        private void ValidatePermissions(Course course, UserDto? loggedUser)
        {
            if (loggedUser != null && course.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Course/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    course.Id, course.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}