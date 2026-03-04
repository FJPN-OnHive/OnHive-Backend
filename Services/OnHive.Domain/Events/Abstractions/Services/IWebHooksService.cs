using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Login;
using System.Text.Json;

namespace OnHive.Events.Domain.Abstractions.Services
{
    public interface IWebHooksService
    {
        Task Receive(string tenantId, string slug, string method, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query, bool authorized);

        Task<WebHookDto?> GetByIdAsync(string webHookId);

        Task<PaginatedResult<WebHookDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<WebHookDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<WebHookDto> SaveAsync(WebHookDto webHookDto, LoggedUserDto? user);

        Task<WebHookDto> CreateAsync(WebHookDto webHookDto, LoggedUserDto? loggedUser);

        Task<WebHookDto?> UpdateAsync(WebHookDto webHookDto, LoggedUserDto? loggedUser);

        Task DeleteById(string webHookId, LoggedUserDto? loggedUser);
    }
}