using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Tenants;

namespace OnHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantThemesRepository : IRepositoryBase<TenantTheme>
    {
        Task<List<TenantTheme>> GetByDomain(string domain, string tenantId);

        Task<TenantTheme> GetCurrentByDomain(string domain, string tenantId);
    }
}