using OnHive.Emails.Domain.Abstractions.Repositories;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Emails.Domain.Mappers;
using OnHive.Emails.Repositories;
using OnHive.Emails.Services;

namespace OnHive.Emails.Api.DependencyInjection
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