using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using System.Text.Json;

namespace OnHive.Users.Domain.Abstractions.Services
{
    public interface IUserGroupsService
    {
        Task<UserGroupDto?> GetByIdAsync(string userGroupId);

        Task<PaginatedResult<UserGroupDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<UserGroupDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<UserGroupDto> SaveAsync(UserGroupDto userGroupDto, LoggedUserDto? user);

        Task<UserGroupDto> CreateAsync(UserGroupDto userGroupDto, LoggedUserDto? loggedUser);

        Task<UserGroupDto?> UpdateAsync(UserGroupDto userGroupDto, LoggedUserDto? loggedUser);

        Task<UserGroupDto?> PatchAsync(JsonDocument userGroupDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteAsync(string userGroupId, LoggedUserDto loggedUser);
    }
}