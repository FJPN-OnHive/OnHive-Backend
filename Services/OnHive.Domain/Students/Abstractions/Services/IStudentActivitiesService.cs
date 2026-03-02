using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Enums.Students;
using System.Text.Json;

namespace EHive.Students.Domain.Abstractions.Services
{
    public interface IStudentActivitiesService
    {
        Task<StudentActivityDto?> GetByIdAsync(string studentId, LoggedUserDto? loggedUser);

        Task<StudentActivityDto?> GetByLoggedUserAsync(LoggedUserDto? loggedUser);

        Task<PaginatedResult<StudentActivityDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<StudentActivityDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<StudentActivityDto?> SaveAsync(StudentActivityDto studentActivityDto, LoggedUserDto? loggedUser);

        Task<StudentActivityDto?> CreateAsync(StudentActivityDto studentActivityDto, LoggedUserDto loggedUser);

        Task<StudentActivityDto?> UpdateAsync(StudentActivityDto studentActivityDto, LoggedUserDto loggedUser);

        Task<StudentActivityDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<List<StudentActivityDto>> GetByCourseId(LoggedUserDto loggedUser, string courseId);

        Task<List<StudentActivityDto>> GetByCourseAndLessonId(LoggedUserDto loggedUser, string courseId, string lessonId);

        Task RegisterActivity(LoggedUserDto loggedUser, StudentCourseDto? course, StudentLessonsDto? lesson, StudentEventTypes type, string eventName, string eventDescription);

        Task RegisterActivity(StudentDto student, StudentCourseDto? course, StudentLessonsDto? lesson, StudentEventTypes type, string eventName, string eventDescription);

        Task<List<StudentActivityDto>> GetByCourseIdLoggedUser(LoggedUserDto user, string courseId);

        Task<List<StudentActivityDto>> GetByCourseAndLessonIdLoggedUser(LoggedUserDto user, string courseId, string lessonId);
    }
}