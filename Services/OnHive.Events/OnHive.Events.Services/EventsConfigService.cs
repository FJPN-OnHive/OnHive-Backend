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
using OnHive.Events.Domain.Extensions;

namespace OnHive.Events.Services
{
    public class EventsConfigService : IEventsConfigService
    {
        private readonly IEventsConfigRepository eventsConfigRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public EventsConfigService(IEventsConfigRepository eventsConfigRepository,
                                   IMapper mapper)
        {
            this.eventsConfigRepository = eventsConfigRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task RegisterEventConfig(EventMessage message)
        {
            var eventConfig = await eventsConfigRepository.GetByKeyAndOrigin(string.Empty, message.Key, message.Origin);
            if (eventConfig == null)
            {
                eventConfig = new EventConfig
                {
                    TenantId = string.Empty,
                    Key = message.Key,
                    Origin = message.Origin,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Description = message.Message,
                    Fields = message.Fields
                           .Select(f => new EventConfigFields { Key = f.Key, Description = f.Value })
                           .ToList()
                };
            }
            else
            {
                eventConfig.Description = message.Message;
                eventConfig.Fields = message.Fields
                           .Select(f => new EventConfigFields { Key = f.Key, Description = f.Value })
                           .ToList();
            }
            await eventsConfigRepository.SaveAsync(eventConfig);
        }

        public async Task<EventConfigDto?> GetByIdAsync(string eventConfigId)
        {
            var eventConfig = await eventsConfigRepository.GetByIdAsync(eventConfigId);
            return mapper.Map<EventConfigDto>(eventConfig);
        }

        public async Task<PaginatedResult<EventConfigDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await eventsConfigRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                result.Itens = await CheckFilteredBaseConfigs(filter, result.Itens, loggedUser?.User?.TenantId);
                return new PaginatedResult<EventConfigDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<EventConfigDto>>(result.Itens)
                };
            }
            return new PaginatedResult<EventConfigDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<EventConfigDto>()
            };
        }

        public async Task<IEnumerable<EventConfigDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var eventConfigs = await eventsConfigRepository.GetAllAsync(loggedUser?.User?.TenantId);
            eventConfigs = await CheckAllBaseConfigs(eventConfigs, loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<EventConfigDto>>(eventConfigs);
        }

        public async Task<EventConfigDto> SaveAsync(EventConfigDto eventConfigDto, LoggedUserDto? loggedUser)
        {
            var eventConfig = mapper.Map<EventConfig>(eventConfigDto);
            ValidatePermissions(eventConfig, loggedUser?.User);
            eventConfig.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            eventConfig.CreatedAt = DateTime.UtcNow;
            eventConfig.CreatedBy = string.IsNullOrEmpty(eventConfig.CreatedBy) ? loggedUser?.User?.Id : eventConfig.CreatedBy;

            var response = await eventsConfigRepository.SaveAsync(eventConfig);
            return mapper.Map<EventConfigDto>(response);
        }

        public async Task<EventConfigDto> CreateAsync(EventConfigDto eventConfigDto, LoggedUserDto? loggedUser)
        {
            if (!eventConfigDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var eventConfig = mapper.Map<EventConfig>(eventConfigDto);
            ValidatePermissions(eventConfig, loggedUser?.User);
            eventConfig.Id = string.Empty;
            eventConfig.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await eventsConfigRepository.SaveAsync(eventConfig, loggedUser.User.Id);
            return mapper.Map<EventConfigDto>(response);
        }

        public async Task<EventConfigDto?> UpdateAsync(EventConfigDto eventConfigDto, LoggedUserDto? loggedUser)
        {
            if (!eventConfigDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var eventConfig = mapper.Map<EventConfig>(eventConfigDto);
            ValidatePermissions(eventConfig, loggedUser?.User);
            var currentEventConfig = await eventsConfigRepository.GetByIdAsync(eventConfig.Id);
            if (currentEventConfig == null || currentEventConfig.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await eventsConfigRepository.SaveAsync(eventConfig, loggedUser.User.Id);
            return mapper.Map<EventConfigDto>(response);
        }

        public async Task<bool> DeleteAsync(string eventConfigId, LoggedUserDto? loggedUser)
        {
            await eventsConfigRepository.DeleteAsync(eventConfigId);
            return true;
        }

        private async Task<List<EventConfig>> CheckFilteredBaseConfigs(RequestFilter filter, List<EventConfig> eventConfigs, string tenantId)
        {
            var baseConfigs = (await eventsConfigRepository.GetByFilterAsync(filter, string.Empty, false)).Itens;
            return await UpdateFromBase(eventConfigs, tenantId, baseConfigs);
        }

        private async Task<List<EventConfig>> CheckAllBaseConfigs(List<EventConfig> eventConfigs, string tenantId)
        {
            var baseConfigs = await eventsConfigRepository.GetAllAsync(string.Empty);
            return await UpdateFromBase(eventConfigs, tenantId, baseConfigs);
        }

        private async Task<List<EventConfig>> UpdateFromBase(List<EventConfig> eventConfigs, string tenantId, List<EventConfig> baseConfigs)
        {
            foreach (var baseConfig in baseConfigs)
            {
                var currentConfig = eventConfigs.FirstOrDefault(ec => ec.Key == baseConfig.Key && ec.Origin == baseConfig.Origin);
                if (currentConfig == null)
                {
                    baseConfig.TenantId = tenantId;
                    baseConfig.Id = string.Empty;
                    eventConfigs.Add(baseConfig);
                    await eventsConfigRepository.SaveAsync(baseConfig);
                }
                else if (!currentConfig.Compare(baseConfig))
                {
                    currentConfig.UpdatedAt = baseConfig.UpdatedAt;
                    currentConfig.UpdatedBy = baseConfig.UpdatedBy;
                    currentConfig.Fields = baseConfig.Fields;
                    await eventsConfigRepository.SaveAsync(currentConfig);
                }
            }
            return eventConfigs;
        }

        private void ValidatePermissions(EventConfig eventConfig, UserDto? loggedUser)
        {
            if (loggedUser != null && eventConfig.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID EventConfig/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    eventConfig.Id, eventConfig.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}