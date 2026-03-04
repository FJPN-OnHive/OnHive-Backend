using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Login;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace OnHive.Events.Domain.Abstractions.Services
{
    public interface IIntegrationsService
    {
        Task MauticAPI(string tenantId, Dictionary<string, string> headers, Dictionary<string, string> query);

        Task MauticAPI(string tenantId, JsonDocument body, Dictionary<string, string> headers, Dictionary<string, string> query);

        Task MauticAPI(string tenantId, Dictionary<string, StringValues> formData, Dictionary<string, string> headers, Dictionary<string, string> query);

        Task<MauticIntegrationDto> GetMauticSettings(LoggedUserDto loggedUser);

        Task<MauticIntegrationDto> CreateMauticSettings(MauticIntegrationDto mauticIntegration, LoggedUserDto loggedUser);

        Task<MauticIntegrationDto> UpdateMauticSettings(MauticIntegrationDto mauticIntegration, LoggedUserDto loggedUser);
    }
}