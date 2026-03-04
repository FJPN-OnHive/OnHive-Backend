using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Events.Domain.Abstractions.Services
{
    public interface IEventsConfigService
    {
        Task<EventConfigDto?> GetByIdAsync(string eventConfigId);

        Task<PaginatedResult<EventConfigDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<EventConfigDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<EventConfigDto> SaveAsync(EventConfigDto eventConfigDto, LoggedUserDto? user);

        Task<EventConfigDto> CreateAsync(EventConfigDto eventConfigDto, LoggedUserDto? loggedUser);

        Task<EventConfigDto?> UpdateAsync(EventConfigDto eventConfigDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteAsync(string eventConfigId, LoggedUserDto? loggedUser);

        Task RegisterEventConfig(EventMessage message);
    }
}