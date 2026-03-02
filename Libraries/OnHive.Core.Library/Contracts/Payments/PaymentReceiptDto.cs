using EHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Payments
{
    public class PaymentReceiptDto
    {
        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonPropertyName("authorizationCode")]
        public string AuthorizationCode { get; set; } = string.Empty;

        [JsonPropertyName("providerKey")]
        public string ProviderKey { get; set; } = string.Empty;

        [JsonPropertyName("paymentToken")]
        public string PaymentToken { get; set; } = string.Empty;

        [JsonPropertyName("proofOfSale")]
        public string ProofOfSale { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public PaymentStatus Status { get; set; }

        [JsonPropertyName("type")]
        public PaymentType Type { get; set; }

        [JsonPropertyName("originalValue")]
        public double OriginalValue { get; set; }

        [JsonPropertyName("paymentValue")]
        public double PaymentValue { get; set; }

        [JsonPropertyName("providerPaymentMessage")]
        public string ProviderPaymentMessage { get; set; } = string.Empty;

        [JsonPropertyName("providerPaymentCode")]
        public string ProviderPaymentCode { get; set; } = string.Empty;

        [JsonPropertyName("checkoutDate")]
        public DateTime CheckoutDate { get; set; }

        [JsonPropertyName("confirmationDate")]
        public DateTime ConfirmationDate { get; set; }

        [JsonPropertyName("cancellationDate")]
        public DateTime CancellationDate { get; set; }

        [JsonPropertyName("refoundDate")]
        public DateTime RefoundDate { get; set; }

        [JsonPropertyName("bankSlipData")]
        public BankSlipDataDto? BankSlipData { get; set; }

        [JsonPropertyName("pixData")]
        public PixDataDto? PixData { get; set; }
    }
}