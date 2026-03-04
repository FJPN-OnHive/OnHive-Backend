using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Teachers;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Teachers.Domain.Abstractions.Services
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