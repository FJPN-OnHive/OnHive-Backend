using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Events;

namespace EHive.Events.Domain.Abstractions.Repositories
{
    public interface IMauticIntegrationRepository : IRepositoryBase<MauticIntegration>
    {
        Task<MauticIntegration> GetMauticIntegrationByTenantId(string tenantId, bool activeOnly);
    }
}