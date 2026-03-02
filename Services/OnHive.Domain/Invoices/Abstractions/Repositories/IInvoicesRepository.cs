using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Invoices;

namespace EHive.Invoices.Domain.Abstractions.Repositories
{
    public interface IInvoicesRepository : IRepositoryBase<Invoice>
    {
        Task<int> GetLastInvoiceNumber(string tenantId, string invoiceSeries);

        Task<IEnumerable<Invoice>> GetPendingInvoices();
    }
}