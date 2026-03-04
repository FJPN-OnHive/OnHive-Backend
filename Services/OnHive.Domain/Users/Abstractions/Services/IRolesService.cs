using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Users.Domain.Abstractions.Services
{
    public interface IRolesService
    {
        Task<RoleDto?> GetByIdAsync(string roleId);

        Task<PaginatedResult<RoleDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<RoleDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<RoleDto> SaveAsync(RoleDto roleDto, LoggedUserDto? user);

        Task<RoleDto> CreateAsync(RoleDto roleDto, LoggedUserDto loggedUser);

        Task<RoleDto?> UpdateAsync(RoleDto roleDto, LoggedUserDto loggedUser);

        Task<RoleDto?> PatchAsync(JsonDocument patchDto, LoggedUserDto loggedUser);

        Task Migrate(bool isProduction);

        Task<List<string>> GetPermissions();
    }
}