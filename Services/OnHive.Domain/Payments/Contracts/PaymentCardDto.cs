using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class PaymentCardDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("cardType")]
        public string CardType { get; set; } = string.Empty;

        [JsonPropertyName("cardOwner")]
        public PaymentClientDto CardHolder { get; set; } = new PaymentClientDto();

        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; } = string.Empty;

        [JsonPropertyName("expirationDate")]
        public string ExpirationDate { get; set; } = string.Empty;

        [JsonPropertyName("securityCode")]
        public string SecurityCode { get; set; } = string.Empty;

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;
    }
}