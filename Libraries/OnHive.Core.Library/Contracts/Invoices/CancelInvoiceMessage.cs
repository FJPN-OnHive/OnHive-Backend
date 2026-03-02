using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Invoices
{
    public class CancelInvoiceMessage
    {
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; }

        [JsonPropertyName("providerKey")]
        public string ProviderKey { get; set; } = string.Empty;
    }
}