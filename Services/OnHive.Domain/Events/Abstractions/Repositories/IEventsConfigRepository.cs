using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Events;

namespace EHive.Events.Domain.Abstractions.Repositories
{
    public interface IEventsConfigRepository : IRepositoryBase<EventConfig>
    {
        Task<List<EventConfig>> GetByOrigin(string tenantId, string origin);

        Task<EventConfig> GetByKeyAndOrigin(string tenantId, string key, string origin);
    }
}