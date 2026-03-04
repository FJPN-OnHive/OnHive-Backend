using OnHive.Configuration.Library.Extensions;
using OnHive.PaymentsCielo.Domain.Models;

namespace OnHive.PaymentsCielo.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureCieloPaymentsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<PaymentCieloSettings>();
            builder.Services.AddServices();
            return builder;
        }
    }
}