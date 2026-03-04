using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public interface IUserProfileService : IServiceBase<UserProfileDto>
    {
        Task<List<UserProfileDto>> GetByUserId(string userId, string token);
    }
}