using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Events.Domain.Abstractions.Services
{
    public interface IEventsService
    {
        Task<EventRegisterDto?> GetByIdAsync(string eventRegisterId);

        Task<PaginatedResult<EventRegisterDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<EventRegisterDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<EventRegisterDto> SaveAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? user);

        Task<EventRegisterDto> CreateAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? loggedUser);

        Task<EventRegisterDto?> UpdateAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteAsync(string eventRegisterId, LoggedUserDto? loggedUser);

        Task ProcessEvent(EventMessage message, bool create);

        Task<PaginatedResult<EventResumeDto>> GetByFilterResumeAsync(RequestFilter filter, LoggedUserDto loggedUser);

        Task<string> HouseKeepingExecute();
    }
}