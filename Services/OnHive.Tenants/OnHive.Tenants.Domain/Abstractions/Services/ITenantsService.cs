using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Tenants.Domain.Abstractions.Services
{
    public interface ITenantsService
    {
        Task<TenantDto?> GetByIdAsync(string tenantId);

        Task<PaginatedResult<TenantDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<TenantDto>> GetAllAsync(UserDto? loggedUser);

        Task<TenantDto> CreateAsync(TenantDto tenantDto, UserDto loggedUser);

        Task<TenantDto?> UpdateAsync(TenantDto tenantDto, UserDto loggedUser);

        Task<TenantDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task Migrate(bool isProduction);

        Task<string> GetByDomainAsync(string subdomain);

        Task<List<TenantResumeDto>> GetAllOpenAsync();

        Task<TenantResumeDto> GetBySlugAsync(string slug);
    }
}