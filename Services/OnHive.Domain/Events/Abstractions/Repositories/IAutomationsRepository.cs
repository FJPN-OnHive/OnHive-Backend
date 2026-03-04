using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Events;

namespace OnHive.Events.Domain.Abstractions.Repositories
{
    public interface IAutomationsRepository : IRepositoryBase<Automation>
    {
        Task<List<Automation>> GetByKey(string tenantId, string key);
    }
}