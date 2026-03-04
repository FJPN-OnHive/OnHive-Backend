using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;

namespace OnHive.Admin.Services
{
    public interface IEventsService : IServiceBase<EventRegisterDto>
    {
        Task<List<EventResumeDto>> GetByFilter(RequestFilter filter, string token);
    }
}