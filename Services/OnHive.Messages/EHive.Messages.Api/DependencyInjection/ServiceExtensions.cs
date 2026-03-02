using EHive.Messages.Domain.Abstractions.Repositories;
using EHive.Messages.Domain.Abstractions.Services;
using EHive.Messages.Domain.Mappers;
using EHive.Messages.Repositories;
using EHive.Messages.Services;

namespace EHive.Messages.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IMessagesRepository, MessagesRepository>()
                .AddTransient<IMessageChannelRepository, MessageChannelsRepository>()
                .AddTransient<IMessageUsersRepository, MessageUsersRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IMessagesService, MessagesService>()
                .AddTransient<IMessageChannelsService, MessageChannelsService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}