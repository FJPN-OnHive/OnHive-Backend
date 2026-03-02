using EHive.Core.Library.Contracts.Tenants;

namespace EHive.Admin.Services
{
    public interface ITenantsService
    {
        Task<TenantDto?> GetTenant(string tenantId, string token);

        Task<List<FeatureDto>> GetFeatures(string token);

        Task<bool> SaveTenant(TenantDto tenant, string token);

        Task<List<TenantResumeDto>> GetTenantsResumesAsync();
    }
}