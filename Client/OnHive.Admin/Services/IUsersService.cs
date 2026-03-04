using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public interface IUsersService : IServiceBase<UserDto>
    {
        Task<bool> ChangePassword(ChangePasswordDto changePasswordDto, string token);

        Task<PaginatedResult<UserDto>> GetByProfilePaginated(string profile, RequestFilter filter, string token);

        Task<UserEmailDto?> ValidateEmail(string email, string userId, string tenantId, string token);

        Task<string> CreateTempPassword(string userId, string token);
    }
}