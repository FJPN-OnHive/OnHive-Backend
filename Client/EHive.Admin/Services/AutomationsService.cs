using EHive.Core.Library.Contracts.Events;

namespace EHive.Admin.Services
{
    public class AutomationsService : ServiceBase<AutomationDto>, IAutomationsService
    {
        public AutomationsService(HttpClient httpClient) : base(httpClient, "/v1/Automation")
        {
        }
    }
}