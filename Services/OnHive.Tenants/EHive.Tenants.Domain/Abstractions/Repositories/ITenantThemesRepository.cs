using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Tenants;

namespace EHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantThemesRepository : IRepositoryBase<TenantTheme>
    {
        Task<List<TenantTheme>> GetByDomain(string domain, string tenantId);

        Task<TenantTheme> GetCurrentByDomain(string domain, string tenantId);
    }
}