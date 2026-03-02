using EHive.Emails.Domain.Abstractions.Repositories;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.Emails.Domain.Mappers;
using EHive.Emails.Repositories;
using EHive.Emails.Services;

namespace EHive.Emails.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IEmailsRepository, EmailsRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IEmailsService, EmailsService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}