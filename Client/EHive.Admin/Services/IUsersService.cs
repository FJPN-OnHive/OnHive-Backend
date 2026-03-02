using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public interface IUsersService : IServiceBase<UserDto>
    {
        Task<bool> ChangePassword(ChangePasswordDto changePasswordDto, string token);

        Task<PaginatedResult<UserDto>> GetByProfilePaginated(string profile, RequestFilter filter, string token);

        Task<UserEmailDto?> ValidateEmail(string email, string userId, string tenantId, string token);

        Task<string> CreateTempPassword(string userId, string token);
    }
}