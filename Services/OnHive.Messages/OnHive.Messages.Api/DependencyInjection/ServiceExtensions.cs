using OnHive.Messages.Domain.Abstractions.Repositories;
using OnHive.Messages.Domain.Abstractions.Services;
using OnHive.Messages.Domain.Mappers;
using OnHive.Messages.Repositories;
using OnHive.Messages.Services;

namespace OnHive.Messages.Api.DependencyInjection
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