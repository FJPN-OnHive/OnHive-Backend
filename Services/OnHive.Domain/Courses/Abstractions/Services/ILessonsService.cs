using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Courses.Domain.Abstractions.Services
{
    public interface ILessonsService
    {
        Task<LessonDto?> GetByIdAsync(string classId, UserDto? loggedUser);

        Task<LessonDto?> GetByIdAsync(string classId);

        Task<PaginatedResult<LessonDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<LessonDto>> GetAllAsync(UserDto? loggedUser);

        Task<LessonDto> SaveAsync(LessonDto classDto, UserDto? user);

        Task<LessonDto> CreateAsync(LessonDto classDto, UserDto loggedUser);

        Task<LessonDto?> UpdateAsync(LessonDto classDto, UserDto loggedUser);

        Task<LessonDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<PaginatedResult<LessonDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto loggedUser);
    }
}