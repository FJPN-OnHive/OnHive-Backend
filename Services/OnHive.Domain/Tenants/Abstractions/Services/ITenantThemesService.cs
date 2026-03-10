using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Tenants.Domain.Abstractions.Services
{
    public interface ITenantThemesService
    {
        Task<TenantThemeDto?> GetByIdAsync(string tenantParameterId);

        Task<TenantThemeDto> GetByTenantId(string tenantId);

        Task<PaginatedResult<TenantThemeDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<TenantThemeDto>> GetAllAsync(UserDto? loggedUser);

        Task<TenantThemeDto> SaveAsync(TenantThemeDto tenantParameterDto, UserDto? loggedUser);

        Task<TenantThemeDto> CreateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser);

        Task<TenantThemeDto?> UpdateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser);

        Task<TenantThemeDto?> PatchAsync(JsonDocument patchDto, UserDto loggedUser);
    }
}