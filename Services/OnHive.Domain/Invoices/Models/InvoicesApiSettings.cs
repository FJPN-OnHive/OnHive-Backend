using System.ComponentModel.DataAnnotations;

namespace OnHive.Invoices.Domain.Models
{
    public class InvoicesApiSettings
    {
        public string? InvoicesAdminPermission { get; set; } = "invoices_admin";

        public List<InvoiceProvider>? InvoiceProviders { get; set; } = [];

        public string PendingInvoiceIntervalCron { get; set; } = "*/5 * * * *";
    }

    public class InvoiceProvider
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}