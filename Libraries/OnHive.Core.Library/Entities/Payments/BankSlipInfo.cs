using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Payments
{
    public class BankSlipInfo
    {
        public string? Number { get; set; }

        public string Assignor { get; set; } = string.Empty;

        public string Demonstrative { get; set; } = string.Empty;

        public DateTime ExpirationDate { get; set; }

        public string Instructions { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;
    }
}