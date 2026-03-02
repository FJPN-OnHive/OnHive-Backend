using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Users;

namespace EHive.Users.Domain.Abstractions.Repositories
{
    public interface IRolesRepository : IRepositoryBase<Role>
    {
        Task<Role?> GetByNameAsync(string roleName, string tenantId);
    }
}