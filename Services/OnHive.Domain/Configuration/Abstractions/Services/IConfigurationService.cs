using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Configuration;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Configuration.Domain.Abstractions.Services
{
    public interface IConfigurationService
    {
        Task<ConfigItemDto?> GetByIdAsync(string id, UserDto? loggedUser);

        Task<ConfigItemDto?> GetByKeyAsync(string key, UserDto? loggedUser);

        Task<ConfigItemDto?> GetByTypeAsync<T>();

        Task<PaginatedResult<ConfigItemDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<ConfigItemDto>> GetAllAsync(UserDto? loggedUser);

        Task<ConfigItemDto> SaveAsync(ConfigItemDto configDto, UserDto? loggedUser);
    }
}