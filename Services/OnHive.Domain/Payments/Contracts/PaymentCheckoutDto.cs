using EHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Payments
{
    public class PaymentCheckoutDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("providerKey")]
        public string ProviderKey { get; set; } = string.Empty;

        [JsonPropertyName("paymentType")]
        public PaymentType PaymentType { get; set; }

        [JsonPropertyName("paymentTypeId")]
        public string PaymentTypeId { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("cardInfo")]
        public PaymentCardDto CardInfo { get; set; } = new();

        [JsonPropertyName("installments")]
        public int Installments { get; set; }

        [JsonPropertyName("paymentClient")]
        public PaymentClientDto PaymentClient { get; set; } = new();

        [JsonPropertyName("fields")]
        public Dictionary<string, string> Fields { get; set; } = new();

        [JsonPropertyName("bankSlipInfo")]
        public BankSlipInfoDto? BankSlipInfo { get; set; }

        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; } = string.Empty;
    }
}