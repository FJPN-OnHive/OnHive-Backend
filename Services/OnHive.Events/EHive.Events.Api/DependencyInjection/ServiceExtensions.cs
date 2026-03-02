using EHive.Events.Api.Workers;
using EHive.Events.Domain.Abstractions.Repositories;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Events.Domain.Mappers;
using EHive.Events.Repositories;
using EHive.Events.Services;
using MailKit.Net.Smtp;

namespace EHive.Events.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IEventsConfigService, EventsConfigService>()
                .AddTransient<IEventsRepository, EventsRepository>()
                .AddTransient<IAutomationsRepository, AutomationsRepository>()
                .AddTransient<IEventsConfigRepository, EventsConfigRepository>()
                .AddTransient<IWebHooksRepository, WebHooksRepository>()
                .AddTransient<IMauticIntegrationRepository, MauticIntegrationRepository>()
                .AddTransient<IEventRegister, EventRegisterAgent>()
                .AddHostedService<CronjobsWorker>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IEventsService, EventsService>()
                .AddTransient<IAutomationsService, AutomationsService>()
                .AddTransient<IEventsConfigService, EventsConfigService>()
                .AddTransient<ISmtpClient, SmtpClient>()
                .AddTransient<IWebHooksService, WebHooksService>()
                .AddTransient<IIntegrationsService, IntegrationsService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}