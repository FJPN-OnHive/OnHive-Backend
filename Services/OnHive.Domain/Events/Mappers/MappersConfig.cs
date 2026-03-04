using AutoMapper;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Entities.Events;

namespace OnHive.Events.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapEventRegisterToEventRegisterDto();
            MapAutomationToAutomationDto();
            MapEventConfigToEventConfigDto();
            MapEventRegisterToEventResumeDto();
            MapWebHookToWebHookDto();
            MapMauticIntegrationsToMauticIntegrationsDto();
        }

        private void MapEventRegisterToEventRegisterDto()
        {
            CreateMap<EventRegister, EventRegisterDto>()
                .ReverseMap();
        }

        private void MapEventRegisterToEventResumeDto()
        {
            CreateMap<EventRegister, EventResumeDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => string.Join(",", src.Tags)));
        }

        private void MapAutomationToAutomationDto()
        {
            CreateMap<AutomationEmail, AutomationEmailDto>()
                .ReverseMap();

            CreateMap<AutomationWebHook, AutomationWebHookDto>()
                .ReverseMap();

            CreateMap<AutomationCondition, AutomationConditionDto>()
                .ReverseMap();

            CreateMap<Automation, AutomationDto>()
                .ReverseMap();
        }

        private void MapEventConfigToEventConfigDto()
        {
            CreateMap<EventConfigFields, EventConfigFieldsDto>()
                .ReverseMap();

            CreateMap<EventConfig, EventConfigDto>()
                .ReverseMap();
        }

        private void MapWebHookToWebHookDto()
        {
            CreateMap<WebHook, WebHookDto>()
                .ReverseMap();

            CreateMap<WebHookStep, WebHookStepDto>()
                .ReverseMap();

            CreateMap<WebHookAction, WebHookActionDto>()
                .ReverseMap();
        }

        private void MapMauticIntegrationsToMauticIntegrationsDto()
        {
            CreateMap<MauticIntegration, MauticIntegrationDto>()
                .ReverseMap();
        }
    }
}