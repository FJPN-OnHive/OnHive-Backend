using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Events;
using System.Text.Json;

namespace EHive.Events.Domain.Abstractions.Repositories
{
    public interface IWebHooksRepository : IRepositoryBase<WebHook>
    {
        Task<WebHook?> GetBySlug(string tenantId, string slug);

        Task ExecuteAction(string tenantId, WebHookAction action, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query);
    }
}