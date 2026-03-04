using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class BankSlipDataDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("barCodeNumber")]
        public string BarCodeNumber { get; set; }

        [JsonPropertyName("digitableLine")]
        public string DigitableLine { get; set; }

        [JsonPropertyName("bank")]
        public int Bank { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonPropertyName("receivedDate")]
        public DateTime ReceivedDate { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}