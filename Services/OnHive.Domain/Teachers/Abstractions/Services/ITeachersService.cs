using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Teachers;
using EHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace EHive.Teachers.Domain.Abstractions.Services
{
    public interface ITeachersService
    {
        Task<TeacherDto?> GetByIdAsync(string teacherId);

        Task<PaginatedResult<TeacherDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<TeacherDto>> GetAllAsync(UserDto? loggedUser);

        Task<TeacherDto> SaveAsync(TeacherDto teacherDto, UserDto? user);

        Task<TeacherDto> CreateAsync(TeacherDto teacherDto, UserDto loggedUser);

        Task<TeacherDto?> UpdateAsync(TeacherDto teacherDto, UserDto loggedUser);

        Task<TeacherDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);
    }
}