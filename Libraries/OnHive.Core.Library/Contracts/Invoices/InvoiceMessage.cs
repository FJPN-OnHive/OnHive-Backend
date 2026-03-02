using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Invoices
{
    public class InvoiceMessage
    {
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("providerKey")]
        public string ProviderKey { get; set; } = string.Empty;
    }
}