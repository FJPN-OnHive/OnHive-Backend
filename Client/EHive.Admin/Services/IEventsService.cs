using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;

namespace EHive.Admin.Services
{
    public interface IEventsService : IServiceBase<EventRegisterDto>
    {
        Task<List<EventResumeDto>> GetByFilter(RequestFilter filter, string token);
    }
}