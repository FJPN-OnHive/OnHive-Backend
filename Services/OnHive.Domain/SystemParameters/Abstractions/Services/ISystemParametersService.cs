using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.SystemParameters;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.SystemParameters.Domain.Abstractions.Services
{
    public interface ISystemParametersService
    {
        Task<SystemParameterDto?> GetByIdAsync(string systemParameterId);

        Task<PaginatedResult<SystemParameterDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<SystemParameterDto>> GetAllAsync();

        Task<SystemParameterDto> SaveAsync(SystemParameterDto systemParameterDto, UserDto? user);

        Task<SystemParameterDto> CreateAsync(SystemParameterDto systemParameterDto, UserDto loggedUser);

        Task<SystemParameterDto?> UpdateAsync(SystemParameterDto systemParameterDto, UserDto loggedUser);

        Task<SystemParameterDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<IEnumerable<SystemParameterDto>> GetByGroupAsync(string group);

        Task Migrate();
    }
}