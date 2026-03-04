using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class PixDataDto
    {
        [JsonPropertyName("acquirerTransactionId")]
        public string AcquirerTransactionId { get; set; }

        [JsonPropertyName("proofOfSale")]
        public string ProofOfSale { get; set; }

        [JsonPropertyName("qrcodeBase64Image")]
        public string QrcodeBase64Image { get; set; }

        [JsonPropertyName("qrCodeString")]
        public string QrCodeString { get; set; }
    }
}