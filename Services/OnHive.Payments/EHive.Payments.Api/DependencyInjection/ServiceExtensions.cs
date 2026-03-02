using EHive.Payments.Domain.Abstractions.Repositories;
using EHive.Payments.Domain.Abstractions.Services;
using EHive.Payments.Domain.Mappers;
using EHive.Payments.Repositories;
using EHive.Payments.Services;

namespace EHive.Payments.Api.DependencyInjection
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