using OnHive.Payments.Domain.Abstractions.Repositories;
using OnHive.Payments.Domain.Abstractions.Services;
using OnHive.Payments.Domain.Mappers;
using OnHive.Payments.Repositories;
using OnHive.Payments.Services;

namespace OnHive.Payments.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IPaymentsRepository, PaymentsRepository>()
                .AddTransient<IBankSlipSettingsRepository, BankSlipSettingsRepository>()
                .AddTransient<IBankSlipNumberControlRepository, BankSlipNumberControlRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IPaymentsService, PaymentsService>()
                .AddTransient<IBankSlipSettingsService, BankSlipSettingsService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}