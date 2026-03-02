namespace EHive.PaymentsCielo.Domain.Models
{
    public class PaymentCieloSettings
    {
        public bool IsActive { get; set; } = true;

        public string? TransactionalUrl { get; set; } = "https://api.cieloecommerce.cielo.com.br/1/sales/";

        public string? QueryUrl { get; set; } = "https://api.cieloecommerce.cielo.com.br/1/sales/";

        public string MerchantId { get; set; } = string.Empty;

        public string MerchantKey { get; set; } = string.Empty;
    }
}