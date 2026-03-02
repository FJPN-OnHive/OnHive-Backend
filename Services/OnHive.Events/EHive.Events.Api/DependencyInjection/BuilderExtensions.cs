using EHive.Configuration.Library.Extensions;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Events.Domain.Models;
using EHive.Events.Services;
using OnHive.Domains.Events.Models;

namespace EHive.Events.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureEventsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<EventsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }

        public static WebApplicationBuilder ConfigureEventRegister(this WebApplicationBuilder builder, string origin)
        {
            return ConfigureEventRegister(builder, origin, [], []);
        }

        public static WebApplicationBuilder ConfigureEventRegister(this WebApplicationBuilder builder, string origin, List<string> defaultTags)
        {
            return ConfigureEventRegister(builder, origin, defaultTags, []);
        }

        public static WebApplicationBuilder ConfigureEventRegister(this WebApplicationBuilder builder, string origin, List<string> defaultTags, Dictionary<string, string> defaultFields)
        {
            builder.Services.AddSingleton(new EventRegisterServiceSettings
            {
                Origin = origin,
                DefaultTags = defaultTags,
                DefaultFields = defaultFields
            });
            builder.Services.AddSingleton<IEventRegister, EventRegisterAgent>();
            return builder;
        }

        public static WebApplicationBuilder ConfigureEvent(this WebApplicationBuilder builder, EventMessage eventConfiguration)
        {
            var service = builder.Services.BuildServiceProvider().GetRequiredService<IEventsConfigService>();
            service.RegisterEventConfig(eventConfiguration);
            return builder;
        }
    }
}