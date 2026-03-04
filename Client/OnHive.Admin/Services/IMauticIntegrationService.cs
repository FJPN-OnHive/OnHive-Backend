using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Tenants;

namespace OnHive.Admin.Services
{
    public interface IMauticIntegrationService
    {
        Task<MauticIntegrationDto?> GetIntegrationSettings(string tenantId, string token);

        Task<bool> SaveIntegrationSettings(MauticIntegrationDto integrationSettings, string token);
    }
}