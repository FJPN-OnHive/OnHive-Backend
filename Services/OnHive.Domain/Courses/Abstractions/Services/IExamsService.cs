using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace EHive.Courses.Domain.Abstractions.Services
{
    public interface IExamsService
    {
        Task<ExamDto?> GetByIdAsync(string examId, UserDto? loggedUser);

        Task<PaginatedResult<ExamDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<ExamDto>> GetAllAsync(UserDto? loggedUser);

        Task<ExamDto> SaveAsync(ExamDto examDto, UserDto? user);

        Task<ExamDto> CreateAsync(ExamDto examDto, UserDto loggedUser);

        Task<ExamDto?> UpdateAsync(ExamDto examDto, UserDto loggedUser);

        Task<ExamDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<ExamDto?> CreateVersionAsync(ExamDto examDto, UserDto loggedUser);

        Task<List<ExamDto>> GetVersionsAsync(string vId, UserDto loggedUser);

        Task<ExamDto> GetVersionAsync(string vId, int versionNumber, UserDto loggedUser);

        Task<ExamDto> GetVersionInternalAsync(string vId, int versionNumber);
    }
}