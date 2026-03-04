using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Enums.Users;

namespace OnHive.Users.Domain.Abstractions.Repositories
{
    public interface IUserProfilesRepository : IRepositoryBase<UserProfile>
    {
        Task<PaginatedResult<UserProfile>> GetByFilterAndTypeAsync(RequestFilter filter, ProfileTypes type, string tenantId);

        Task<List<UserProfile>> GetByUserIdAsync(string userId);
    }
}