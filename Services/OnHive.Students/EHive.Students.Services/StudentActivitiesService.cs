using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Entities.Students;
using EHive.Core.Library.Enums.Students;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.Students.Domain.Abstractions.Repositories;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Students.Domain.Models;
using Serilog;
using System.Text.Json;

namespace EHive.Students.Services
{
    public class StudentActivitiesService : IStudentActivitiesService
    {
        private readonly IStudentActivitiesRepository studentActivitiesRepository;
        private readonly StudentsApiSettings studentsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public StudentActivitiesService(IStudentActivitiesRepository studentActivitiesRepository,
                               StudentsApiSettings studentsApiSettings,
                               IMapper mapper)
        {
            this.studentActivitiesRepository = studentActivitiesRepository;
            this.studentsApiSettings = studentsApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<List<StudentActivityDto>> GetByCourseId(LoggedUserDto loggedUser, string courseId)
        {
            var studentActivity = await studentActivitiesRepository.GetByCourseIdAsync(courseId, loggedUser.User.TenantId);
            if (studentActivity == null)
            {
                return null;
            }
            studentActivity.ForEach(x => ValidatePermissions(x, loggedUser?.User));
            return mapper.Map<List<StudentActivityDto>>(studentActivity);
        }

        public async Task<List<StudentActivityDto>> GetByCourseAndLessonId(LoggedUserDto loggedUser, string courseId, string lessonId)
        {
            var studentActivity = await studentActivitiesRepository.GetByCourseandLessonIdAsync(courseId, lessonId, loggedUser.User.TenantId);
            if (studentActivity == null)
            {
                return null;
            }
            studentActivity.ForEach(x => ValidatePermissions(x, loggedUser?.User));
            return mapper.Map<List<StudentActivityDto>>(studentActivity);
        }

        public async Task<List<StudentActivityDto>> GetByCourseIdLoggedUser(LoggedUserDto loggedUser, string courseId)
        {
            var studentActivity = await studentActivitiesRepository.GetByCourseIdUserAsync(loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser)), courseId, loggedUser.User.TenantId);
            if (studentActivity == null)
            {
                return null;
            }
            studentActivity.ForEach(x => ValidatePermissions(x, loggedUser?.User));
            return mapper.Map<List<StudentActivityDto>>(studentActivity);
        }

        public async Task<List<StudentActivityDto>> GetByCourseAndLessonIdLoggedUser(LoggedUserDto loggedUser, string courseId, string lessonId)
        {
            var studentActivity = await studentActivitiesRepository.GetByCourseandLessonIdUserAsync(loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser)), courseId, lessonId, loggedUser.User.TenantId);
            if (studentActivity == null)
            {
                return null;
            }
            studentActivity.ForEach(x => ValidatePermissions(x, loggedUser?.User));
            return mapper.Map<List<StudentActivityDto>>(studentActivity);
        }

        public async Task RegisterActivity(StudentDto student, StudentCourseDto? course, StudentLessonsDto? lesson, StudentEventTypes type, string eventName, string eventDescription)
        {
            var studentActivity = new StudentActivity
            {
                UserId = student.UserId,
                CourseId = course?.Id ?? string.Empty,
                CourseName = course?.Name ?? string.Empty,
                LessonId = lesson?.Id ?? string.Empty,
                LessonName = lesson?.Name ?? string.Empty,
                EventType = type,
                ActivityDate = DateTime.UtcNow,
                Event = eventName,
                EventDescription = eventDescription,
                IsActive = true,
                TenantId = student.TenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = student.UserId
            };
            await studentActivitiesRepository.SaveAsync(studentActivity, student.UserId);
        }

        public async Task RegisterActivity(LoggedUserDto loggedUser, StudentCourseDto? course, StudentLessonsDto? lesson, StudentEventTypes type, string eventName, string eventDescription)
        {
            var studentActivity = new StudentActivity
            {
                UserId = loggedUser.User.Id,
                CourseId = course?.Id ?? string.Empty,
                CourseName = course?.Name ?? string.Empty,
                LessonId = lesson?.Id ?? string.Empty,
                LessonName = lesson?.Name ?? string.Empty,
                EventType = type,
                ActivityDate = DateTime.UtcNow,
                Event = eventName,
                EventDescription = eventDescription,
                IsActive = true,
                TenantId = loggedUser.User.TenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = loggedUser.User.Id ?? string.Empty
            };
            await studentActivitiesRepository.SaveAsync(studentActivity, loggedUser.User.Id);
        }

        public async Task<StudentActivityDto?> GetByIdAsync(string studentId, LoggedUserDto? loggedUser)
        {
            var studentActivity = await studentActivitiesRepository.GetByIdAsync(studentId);
            if (studentActivity == null)
            {
                return null;
            }
            ValidatePermissions(studentActivity, loggedUser?.User);
            return mapper.Map<StudentActivityDto>(studentActivity);
        }

        public async Task<StudentActivityDto?> GetByLoggedUserAsync(LoggedUserDto? loggedUser)
        {
            var studentActivity = await studentActivitiesRepository.GetAllByUserIdAsync(loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser)));
            if (studentActivity == null)
            {
                return null;
            }
            return mapper.Map<StudentActivityDto>(studentActivity);
        }

        public async Task<PaginatedResult<StudentActivityDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await studentActivitiesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<StudentActivityDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<StudentActivityDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StudentActivityDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = []
            };
        }

        public async Task<IEnumerable<StudentActivityDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var students = await studentActivitiesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<StudentActivityDto>>(students);
        }

        public async Task<StudentActivityDto> SaveAsync(StudentActivityDto studentActivityDto, LoggedUserDto? loggedUser)
        {
            var studentActivity = mapper.Map<StudentActivity>(studentActivityDto);
            ValidatePermissions(studentActivity, loggedUser?.User);
            studentActivity.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            studentActivity.IsActive = true;
            studentActivity.CreatedAt = DateTime.UtcNow;
            studentActivity.CreatedBy = string.IsNullOrEmpty(studentActivity.CreatedBy) ? loggedUser.User.Id : studentActivity.CreatedBy;

            var response = await studentActivitiesRepository.SaveAsync(studentActivity);
            return mapper.Map<StudentActivityDto>(response);
        }

        public async Task<StudentActivityDto> CreateAsync(StudentActivityDto studentActivityDto, LoggedUserDto loggedUser)
        {
            var studentActivity = mapper.Map<StudentActivity>(studentActivityDto);
            ValidatePermissions(studentActivity, loggedUser.User);
            studentActivity.Id = string.Empty;
            studentActivity.TenantId = loggedUser.User.TenantId;
            studentActivity.IsActive = true;
            var response = await studentActivitiesRepository.SaveAsync(studentActivity, loggedUser.User.Id);
            return mapper.Map<StudentActivityDto>(response);
        }

        public async Task<StudentActivityDto?> UpdateAsync(StudentActivityDto studentActivityDto, LoggedUserDto loggedUser)
        {
            var studentActivity = mapper.Map<StudentActivity>(studentActivityDto);
            ValidatePermissions(studentActivity, loggedUser.User);
            var currentStudent = await studentActivitiesRepository.GetByIdAsync(studentActivity.Id);
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await studentActivitiesRepository.SaveAsync(studentActivity, loggedUser.User.Id);
            return mapper.Map<StudentActivityDto>(response);
        }

        public async Task<StudentActivityDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentStudent = await studentActivitiesRepository.GetByIdAsync(patch.GetId());
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentStudent = patch.PatchEntity(currentStudent);
            ValidatePermissions(currentStudent, loggedUser.User);
            if (!mapper.Map<StudentActivityDto?>(currentStudent).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await studentActivitiesRepository.SaveAsync(currentStudent, loggedUser.User.Id);
            return mapper.Map<StudentActivityDto>(response);
        }

        private void ValidatePermissions(StudentActivity student, UserDto? loggedUser)
        {
            if (loggedUser == null
                || student.TenantId != loggedUser.TenantId
                || (loggedUser.Id != student.UserId && !loggedUser.Permissions.Contains(studentsApiSettings.StudentsAdminPermission ?? string.Empty)))
            {
                logger.Warning("Unauthorized update mismatch tenantID Student/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    student.Id, student.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}