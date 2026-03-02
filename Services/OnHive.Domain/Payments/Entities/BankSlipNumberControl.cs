namespace EHive.Core.Library.Entities.Payments
{
    public class BankSlipNumberControl : EntityBase
    {
        public int LastNumber { get; set; }

        public string LastOrderId { get; set; } = string.Empty;

        public string LastPaymentId { get; set; } = string.Empty;

        public string ProviderKey { get; set; } = string.Empty;
    }
}