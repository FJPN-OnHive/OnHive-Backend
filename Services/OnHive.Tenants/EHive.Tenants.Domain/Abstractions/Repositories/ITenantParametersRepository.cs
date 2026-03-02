using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Tenants;

namespace EHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantParametersRepository : IRepositoryBase<TenantParameter>
    {
        Task<TenantParameter> GetByKey(string group, string key, string tenantId);

        Task<IEnumerable<TenantParameter>> GetByGroup(string group, string tenantId);
    }
}