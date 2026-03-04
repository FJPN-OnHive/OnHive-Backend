using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Events;

namespace OnHive.Events.Domain.Abstractions.Repositories
{
    public interface IEventsRepository : IRepositoryBase<EventRegister>
    {
        Task<long> RemoveNonPersistentOlderThanAsync(DateTime referenceDate);

        Task<List<EventRegister>> GetByOrigin(string tenantId, string origin);

        Task<List<EventRegister>> GetByKey(string tenantId, string key);

        Task<List<EventRegister>> GetPeriod(string tenantId, DateTime initial, DateTime final);

        Task<List<EventRegister>> GetFilter(string tenantId, DateTime initial, DateTime final, string? origin, string? key);
    }
}