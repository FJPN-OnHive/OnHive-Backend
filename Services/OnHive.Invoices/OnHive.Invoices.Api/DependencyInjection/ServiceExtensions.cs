using OnHive.Invoices.Api.Workers;
using OnHive.Invoices.Domain.Abstractions.Repositories;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Mappers;
using OnHive.Invoices.Repositories;
using OnHive.Invoices.Services;

namespace OnHive.Invoices.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IInvoicesRepository, InvoicesRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IInvoicesService, InvoicesService>();
        }

        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            return services
                .AddHostedService<PendingInvoicesWorker>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}