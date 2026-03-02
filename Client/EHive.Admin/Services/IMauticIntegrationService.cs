using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Contracts.Tenants;

namespace EHive.Admin.Services
{
    public interface IMauticIntegrationService
    {
        Task<MauticIntegrationDto?> GetIntegrationSettings(string tenantId, string token);

        Task<bool> SaveIntegrationSettings(MauticIntegrationDto integrationSettings, string token);
    }
}