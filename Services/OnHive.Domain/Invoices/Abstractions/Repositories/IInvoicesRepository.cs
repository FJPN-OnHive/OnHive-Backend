using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Invoices;

namespace OnHive.Invoices.Domain.Abstractions.Repositories
{
    public interface IInvoicesRepository : IRepositoryBase<Invoice>
    {
        Task<int> GetLastInvoiceNumber(string tenantId, string invoiceSeries);

        Task<IEnumerable<Invoice>> GetPendingInvoices();
    }
}