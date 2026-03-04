using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Dict;

namespace OnHive.Dict.Domain.Abstractions.Repositories
{
    public interface IDictRepository : IRepositoryBase<ValueRegistry>
    {
        Task<ValueRegistry> GetByGroupAndKeyAsync(string tenantId, string group, string key);

        Task<List<string>> GetGroupsAsync(string tenantId);

        Task<List<string>> GetKeysAsync(string tenantId, string group);
    }
}