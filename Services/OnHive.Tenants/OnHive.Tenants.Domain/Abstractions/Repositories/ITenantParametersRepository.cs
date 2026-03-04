using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Tenants;

namespace OnHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantParametersRepository : IRepositoryBase<TenantParameter>
    {
        Task<TenantParameter> GetByKey(string group, string key, string tenantId);

        Task<IEnumerable<TenantParameter>> GetByGroup(string group, string tenantId);
    }
}