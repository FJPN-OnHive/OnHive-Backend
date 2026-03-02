using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Dict;

namespace EHive.Dict.Domain.Abstractions.Repositories
{
    public interface IDictRepository : IRepositoryBase<ValueRegistry>
    {
        Task<ValueRegistry> GetByGroupAndKeyAsync(string tenantId, string group, string key);

        Task<List<string>> GetGroupsAsync(string tenantId);

        Task<List<string>> GetKeysAsync(string tenantId, string group);
    }
}