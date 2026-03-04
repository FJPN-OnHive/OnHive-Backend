using OnHive.Core.Library.Contracts.Events;

namespace OnHive.Admin.Services
{
    public class WebhookService : ServiceBase<WebHookDto>, IWebhookService
    {
        public WebhookService(HttpClient httpClient) : base(httpClient, "/v1/WebHook")
        {
        }
    }
}