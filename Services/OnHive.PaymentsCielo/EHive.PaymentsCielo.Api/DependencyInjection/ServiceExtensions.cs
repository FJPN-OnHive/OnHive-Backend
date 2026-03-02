using EHive.PaymentsCielo.Domain.Abstractions.Services;
using EHive.PaymentsCielo.Services;

namespace EHive.PaymentsCielo.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
               .AddTransient<IPaymentCieloService, PaymentCieloService>();
        }
    }
}