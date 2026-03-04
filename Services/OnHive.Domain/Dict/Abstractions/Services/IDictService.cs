using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Dict;
using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Dict.Domain.Abstractions.Services
{
    public interface IDictService
    {
        Task<ValueRegistryDto?> GetByIdAsync(string valuesId);

        Task<PaginatedResult<ValueRegistryDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<ValueRegistryDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<ValueRegistryDto> SaveAsync(ValueRegistryDto valuesDto, LoggedUserDto? user);

        Task<ValueRegistryDto> CreateAsync(ValueRegistryDto valuesDto, LoggedUserDto? loggedUser);

        Task<ValueRegistryDto?> UpdateAsync(ValueRegistryDto valuesDto, LoggedUserDto? loggedUser);

        Task<ValueRegistryDto> GetByGroupAndKeyAsync(string tenantId, string group, string key);

        Task<bool> DeleteAsync(string valueId, LoggedUserDto loggedUser);

        Task<List<string>> GetGroupsAsync(string tenantId);

        Task<List<string>> GetKeysAsync(string tenantId, string group);
    }
}