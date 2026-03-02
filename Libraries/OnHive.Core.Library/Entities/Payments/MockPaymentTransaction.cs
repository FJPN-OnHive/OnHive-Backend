using EHive.Core.Library.Contracts.Payments;

namespace EHive.Core.Library.Entities.Payments
{
    public class MockPaymentTransaction : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string PaymentId { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public PaymentCheckoutDto Checkout { get; set; } = new();

        public PaymentReceiptDto Receipt { get; set; } = new();
    }
}