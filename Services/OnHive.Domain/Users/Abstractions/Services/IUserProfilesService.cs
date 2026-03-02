using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Enums.Users;

namespace EHive.Users.Domain.Abstractions.Services
{
    public interface IUserProfilesService
    {
        Task<UserProfileDto?> GetByIdAsync(string userProfileId);

        Task<PaginatedResult<UserProfileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<UserProfileDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<UserProfileDto> SaveAsync(UserProfileDto userProfileDto, LoggedUserDto? user);

        Task<UserProfileDto> CreateAsync(UserProfileDto userProfileDto, LoggedUserDto? loggedUser);

        Task<UserProfileDto?> UpdateAsync(UserProfileDto userProfileDto, LoggedUserDto? loggedUser);

        Task<List<UserProfileCompleteDto>> GetByUserIdAsync(string userId);

        Task<List<UserProfileDto>> GetByUserIdManagementAsync(string userId);

        Task<PaginatedResult<UserProfileCompleteDto>> GetByTypeAsync(RequestFilter filter, ProfileTypes type, string tenantId);
    }
}