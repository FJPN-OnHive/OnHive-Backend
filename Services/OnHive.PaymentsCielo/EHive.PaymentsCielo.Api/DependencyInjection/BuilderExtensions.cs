using EHive.Configuration.Library.Extensions;
using EHive.PaymentsCielo.Domain.Models;

namespace EHive.PaymentsCielo.Api.DependencyInjection
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