using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Users;

namespace OnHive.Users.Domain.Abstractions.Repositories
{
    public interface IRolesRepository : IRepositoryBase<Role>
    {
        Task<Role?> GetByNameAsync(string roleName, string tenantId);
    }
}