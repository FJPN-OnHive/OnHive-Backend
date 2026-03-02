using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Users;

namespace EHive.Users.Domain.Abstractions.Repositories
{
    public interface IUserProfilesRepository : IRepositoryBase<UserProfile>
    {
        Task<PaginatedResult<UserProfile>> GetByFilterAndTypeAsync(RequestFilter filter, ProfileTypes type, string tenantId);

        Task<List<UserProfile>> GetByUserIdAsync(string userId);
    }
}