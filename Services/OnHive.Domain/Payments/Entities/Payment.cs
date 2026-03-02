using EHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Payments
{
    public class Payment : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string OrderNumber { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string ProviderKey { get; set; } = string.Empty;

        public PaymentType PaymentType { get; set; } = PaymentType.BankSlip;

        public string PaymentToken { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; }

        public double OriginalValue { get; set; }

        public double PaymentValue { get; set; }

        public string ProviderReceiptKey { get; set; } = string.Empty;

        public string ProviderPaymentMessage { get; set; } = string.Empty;

        public DateTime CheckoutDate { get; set; }

        public DateTime ConfirmationDate { get; set; }

        public DateTime CancellationDate { get; set; }

        public DateTime RefoundDate { get; set; }

        public Dictionary<string, string> Fields { get; set; } = new();

        public PaymentReceipt? LastReceipt { get; set; }

        public BankSlipInfo? BankSlipInfo { get; set; }
    }
}