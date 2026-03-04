using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Entities.Payments
{
    public class BankSlipData
    {
        public string Url { get; set; }

        public string BarCodeNumber { get; set; }

        public string DigitableLine { get; set; }

        public int Bank { get; set; }

        public DateTime ExpirationDate { get; set; }

        public DateTime ReceivedDate { get; set; }

        public string Currency { get; set; }

        public string Country { get; set; }
    }
}