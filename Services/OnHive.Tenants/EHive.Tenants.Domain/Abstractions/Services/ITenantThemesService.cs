using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace EHive.Tenants.Domain.Abstractions.Services
{
    public interface ITenantThemesService
    {
        Task<TenantThemeDto?> GetByIdAsync(string tenantParameterId);

        Task<IEnumerable<TenantThemeDto>> GetByDomain(string domain, string tenantId);

        Task<TenantThemeDto> GetCurrentByDomain(string domain, string tenantId);

        Task<PaginatedResult<TenantThemeDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<TenantThemeDto>> GetAllAsync(UserDto? loggedUser);

        Task<TenantThemeDto> SaveAsync(TenantThemeDto tenantParameterDto, UserDto? loggedUser);

        Task<TenantThemeDto> CreateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser);

        Task<TenantThemeDto?> UpdateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser);

        Task<TenantThemeDto?> PatchAsync(JsonDocument patchDto, UserDto loggedUser);
    }
}