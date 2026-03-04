using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Events;

namespace OnHive.Events.Domain.Abstractions.Repositories
{
    public interface IEventsConfigRepository : IRepositoryBase<EventConfig>
    {
        Task<List<EventConfig>> GetByOrigin(string tenantId, string origin);

        Task<EventConfig> GetByKeyAndOrigin(string tenantId, string key, string origin);
    }
}