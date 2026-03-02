using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Invoices;
using EHive.Core.Library.Contracts.Login;
using System.Text.Json;

namespace EHive.Invoices.Domain.Abstractions.Services
{
    public interface IInvoicesService
    {
        Task<InvoiceDto?> GetByIdAsync(string invoiceId);

        Task<PaginatedResult<InvoiceDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<InvoiceDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<InvoiceDto> SaveAsync(InvoiceDto invoiceDto, LoggedUserDto? user);

        Task<InvoiceDto> CreateAsync(InvoiceDto invoiceDto, LoggedUserDto? loggedUser);

        Task<InvoiceDto?> UpdateAsync(InvoiceDto invoiceDto, LoggedUserDto? loggedUser);

        Task<InvoiceDto?> UpdateAsync(JsonDocument patch, LoggedUserDto? loggedUser);

        Task<InvoiceDto?> InitializeInvoice(InvoiceMessage invoiceMessage);

        Task VerifyPendingInvoices();
    }
}