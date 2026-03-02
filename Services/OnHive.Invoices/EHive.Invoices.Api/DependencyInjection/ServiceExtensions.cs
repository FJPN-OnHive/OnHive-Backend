using EHive.Invoices.Api.Workers;
using EHive.Invoices.Domain.Abstractions.Repositories;
using EHive.Invoices.Domain.Abstractions.Services;
using EHive.Invoices.Domain.Mappers;
using EHive.Invoices.Repositories;
using EHive.Invoices.Services;

namespace EHive.Invoices.Api.DependencyInjection
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