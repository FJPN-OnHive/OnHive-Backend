using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Events;

namespace EHive.Events.Domain.Abstractions.Repositories
{
    public interface IAutomationsRepository : IRepositoryBase<Automation>
    {
        Task<List<Automation>> GetByKey(string tenantId, string key);
    }
}