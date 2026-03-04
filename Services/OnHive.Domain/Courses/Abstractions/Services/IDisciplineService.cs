using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Courses.Domain.Abstractions.Services
{
    public interface IDisciplineService
    {
        Task<DisciplineDto?> GetByIdAsync(string disciplineId, UserDto? loggedUser);

        Task<DisciplineDto?> GetByIdAsync(string disciplineId);

        Task<PaginatedResult<DisciplineDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<DisciplineDto>> GetAllAsync(UserDto? loggedUser);

        Task<DisciplineDto> SaveAsync(DisciplineDto disciplineDto, UserDto? user);

        Task<DisciplineDto> CreateAsync(DisciplineDto disciplineDto, UserDto loggedUser);

        Task<DisciplineDto?> UpdateAsync(DisciplineDto disciplineDto, UserDto loggedUser);

        Task<DisciplineDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<PaginatedResult<DisciplineDto>> GetByIdsAsync(List<string> lessonIds, RequestFilter filter, UserDto? loggedUser);
    }
}