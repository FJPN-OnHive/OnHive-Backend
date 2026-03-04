using AutoMapper;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Validations.Common;
using OnHive.Core.Library.Entities.Events;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Events.Domain.Abstractions.Repositories;
using OnHive.Events.Domain.Abstractions.Services;
using Serilog;

namespace OnHive.Events.Services
{
    public class AutomationsService : IAutomationsService
    {
        private readonly IAutomationsRepository automationsRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public AutomationsService(IAutomationsRepository automationsRepository, IMapper mapper)
        {
            this.automationsRepository = automationsRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<AutomationDto?> GetByIdAsync(string automationId)
        {
            var automation = await automationsRepository.GetByIdAsync(automationId);
            return mapper.Map<AutomationDto>(automation);
        }

        public async Task<PaginatedResult<AutomationDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await automationsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<AutomationDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<AutomationDto>>(result.Itens)
                };
            }
            return new PaginatedResult<AutomationDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<AutomationDto>()
            };
        }

        public async Task<IEnumerable<AutomationDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var automations = await automationsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<AutomationDto>>(automations);
        }

        public async Task<AutomationDto> SaveAsync(AutomationDto automationDto, LoggedUserDto? loggedUser)
        {
            var automation = mapper.Map<Automation>(automationDto);
            ValidatePermissions(automation, loggedUser?.User);
            automation.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            automation.CreatedAt = DateTime.UtcNow;
            automation.CreatedBy = string.IsNullOrEmpty(automation.CreatedBy) ? loggedUser?.User?.Id : automation.CreatedBy;
            var response = await automationsRepository.SaveAsync(automation);
            return mapper.Map<AutomationDto>(response);
        }

        public async Task<AutomationDto> CreateAsync(AutomationDto automationDto, LoggedUserDto? loggedUser)
        {
            if (!automationDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var automation = mapper.Map<Automation>(automationDto);
            ValidatePermissions(automation, loggedUser?.User);
            automation.Id = string.Empty;
            automation.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await automationsRepository.SaveAsync(automation, loggedUser.User.Id);
            return mapper.Map<AutomationDto>(response);
        }

        public async Task<AutomationDto?> UpdateAsync(AutomationDto automationDto, LoggedUserDto? loggedUser)
        {
            if (!automationDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var automation = mapper.Map<Automation>(automationDto);
            ValidatePermissions(automation, loggedUser?.User);
            var currentAutomation = await automationsRepository.GetByIdAsync(automation.Id);
            if (currentAutomation == null || currentAutomation.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await automationsRepository.SaveAsync(automation, loggedUser.User.Id);
            return mapper.Map<AutomationDto>(response);
        }

        public async Task<bool> DeleteAsync(string automationId, LoggedUserDto? loggedUser)
        {
            await automationsRepository.DeleteAsync(automationId);
            return true;
        }

        private void ValidatePermissions(Automation automation, UserDto? loggedUser)
        {
            if (loggedUser != null && automation.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Automation/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    automation.Id, automation.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}