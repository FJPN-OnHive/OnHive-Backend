using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public interface IUserProfileService : IServiceBase<UserProfileDto>
    {
        Task<List<UserProfileDto>> GetByUserId(string userId, string token);
    }
}