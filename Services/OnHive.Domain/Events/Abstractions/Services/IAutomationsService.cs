using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Events.Domain.Abstractions.Services
{
    public interface IAutomationsService
    {
        Task<AutomationDto?> GetByIdAsync(string automationId);

        Task<PaginatedResult<AutomationDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<AutomationDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<AutomationDto> SaveAsync(AutomationDto automationDto, LoggedUserDto? user);

        Task<AutomationDto> CreateAsync(AutomationDto automationDto, LoggedUserDto? loggedUser);

        Task<AutomationDto?> UpdateAsync(AutomationDto automationDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteAsync(string automationId, LoggedUserDto? loggedUser);
    }
}