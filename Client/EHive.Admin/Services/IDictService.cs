using EHive.Core.Library.Contracts.Dict;

namespace EHive.Admin.Services
{
    public interface IDictService : IServiceBase<ValueRegistryDto>
    {
        Task<ValueRegistryDto?> GetCompleteDataAsync(string tenantId, string group, string key);
    }
}