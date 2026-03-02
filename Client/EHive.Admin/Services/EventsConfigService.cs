using EHive.Core.Library.Contracts.Events;

namespace EHive.Admin.Services
{
    public class EventsConfigService : ServiceBase<EventConfigDto>, IEventsConfigService
    {
        public EventsConfigService(HttpClient httpClient) : base(httpClient, "/v1/EventConfig")
        {
        }
    }
}