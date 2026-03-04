using OnHive.Core.Library.Contracts.Events;

namespace OnHive.Admin.Services
{
    public class AutomationsService : ServiceBase<AutomationDto>, IAutomationsService
    {
        public AutomationsService(HttpClient httpClient) : base(httpClient, "/v1/Automation")
        {
        }
    }
}