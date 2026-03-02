using EHive.Configuration.Library.Extensions;
using EHive.Invoices.Domain.Models;

namespace EHive.Invoices.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureInvoicesApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<InvoicesApiSettings>();
            builder.Services.AddServices();
            //builder.Services.AddWorkers();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            return builder;
        }
    }
}