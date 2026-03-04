using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Tenants.Domain.Abstractions.Services
{
    public interface ITenantParametersService
    {
        Task<TenantParameterDto?> GetByIdAsync(string tenantParameterId);

        Task<IEnumerable<TenantParameterDto>> GetByGroup(string group, string tenantId);

        Task<TenantParameterDto?> GetByKey(string group, string key, string tenantId);

        Task<PaginatedResult<TenantParameterDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<TenantParameterDto>> GetAllAsync(UserDto? loggedUser);

        Task<TenantParameterDto> SaveAsync(TenantParameterDto tenantParameterDto, UserDto? loggedUser);

        Task<TenantParameterDto> CreateAsync(TenantParameterDto tenantParameterDto, UserDto loggedUser);

        Task<TenantParameterDto?> UpdateAsync(TenantParameterDto tenantParameterDto, UserDto loggedUser);

        Task<TenantParameterDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);
    }
}