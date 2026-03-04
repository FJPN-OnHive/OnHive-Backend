using OnHive.Events.Api.Workers;
using OnHive.Events.Domain.Abstractions.Repositories;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Events.Domain.Mappers;
using OnHive.Events.Repositories;
using OnHive.Events.Services;
using MailKit.Net.Smtp;

namespace OnHive.Events.Api.DependencyInjection
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