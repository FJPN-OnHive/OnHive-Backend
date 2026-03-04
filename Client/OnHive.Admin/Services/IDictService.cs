using OnHive.Core.Library.Contracts.Dict;

namespace OnHive.Admin.Services
{
    public interface IDictService : IServiceBase<ValueRegistryDto>
    {
        Task<ValueRegistryDto?> GetCompleteDataAsync(string tenantId, string group, string key);
    }
}