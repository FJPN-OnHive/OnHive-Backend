using EHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Payments
{
    public class PaymentReceipt
    {
        public string PaymentId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string OrderNumber { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;

        public string AuthorizationCode { get; set; } = string.Empty;

        public string ProviderKey { get; set; } = string.Empty;

        public string PaymentToken { get; set; } = string.Empty;

        public string ProofOfSale { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; }

        public PaymentType Type { get; set; }

        public double OriginalValue { get; set; }

        public double PaymentValue { get; set; }

        public string ProviderPaymentMessage { get; set; } = string.Empty;

        public string ProviderPaymentCode { get; set; } = string.Empty;

        public DateTime CheckoutDate { get; set; }

        public DateTime ConfirmationDate { get; set; }

        public DateTime CancellationDate { get; set; }

        public DateTime RefoundDate { get; set; }

        public BankSlipData? BankSlipData { get; set; }
    }
}