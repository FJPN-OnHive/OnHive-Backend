using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Events;

namespace OnHive.Events.Domain.Abstractions.Repositories
{
    public interface IMauticIntegrationRepository : IRepositoryBase<MauticIntegration>
    {
        Task<MauticIntegration> GetMauticIntegrationByTenantId(string tenantId, bool activeOnly);
    }
}