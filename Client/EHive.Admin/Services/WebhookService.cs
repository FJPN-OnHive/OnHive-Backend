using EHive.Core.Library.Contracts.Events;

namespace EHive.Admin.Services
{
    public class WebhookService : ServiceBase<WebHookDto>, IWebhookService
    {
        public WebhookService(HttpClient httpClient) : base(httpClient, "/v1/WebHook")
        {
        }
    }
}