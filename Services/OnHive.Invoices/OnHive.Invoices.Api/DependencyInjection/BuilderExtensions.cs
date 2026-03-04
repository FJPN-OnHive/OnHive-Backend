using OnHive.Configuration.Library.Extensions;
using OnHive.Invoices.Domain.Models;

namespace OnHive.Invoices.Api.DependencyInjection
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