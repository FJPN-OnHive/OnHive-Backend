using OnHive.PaymentsCielo.Domain.Abstractions.Services;
using OnHive.PaymentsCielo.Services;

namespace OnHive.PaymentsCielo.Api.DependencyInjection
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