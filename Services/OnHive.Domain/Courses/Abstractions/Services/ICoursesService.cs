using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Enums.Common;
using System.Text.Json;

namespace OnHive.Courses.Domain.Abstractions.Services
{
    public interface ICoursesService
    {
        Task<CourseDto?> GetByIdAsync(string courseId);

        Task<CourseDto?> GetByIdAsync(string courseId, UserDto? loggedUser);

        Task<PaginatedResult<CourseDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<PaginatedResult<CourseResumeDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId);

        Task<IEnumerable<CourseDto>> GetAllAsync(UserDto? loggedUser);

        Task<CourseDto> SaveAsync(CourseDto courseDto, UserDto? user);

        Task<CourseDto> CreateAsync(CourseDto courseDto, UserDto loggedUser);

        Task<CourseDto?> UpdateAsync(CourseDto courseDto, UserDto loggedUser);

        Task<CourseDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<Stream> GetExportData(ExportFormats exportFormat, string tenantId, bool v);

        Task<PaginatedResult<CourseResumeDto>> GetByFilterByUserAsync(RequestFilter filter, UserDto user);

        Task<CourseDto?> GetByIdInternalAsync(string courseId);

        Task<List<CourseDto>> GetAllByTenantInternalAsync(string tenantId);

        Task<PaginatedResult<CourseDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto loggedUser);
    }
}