using OnHive.Core.Library.Enums.Payments;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Payments
{
    public class PaymentTypeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("paymentType")]
        public PaymentType PaymentType { get; set; }

        [JsonPropertyName("status")]
        public PaymentTypeStatus Status { get; set; }

        [JsonPropertyName("secureThumbnail")]
        public string SecureThumbnail { get; set; } = string.Empty;

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; } = string.Empty;

        [JsonPropertyName("settings")]
        public Settings? Settings { get; set; }

        [JsonPropertyName("additionalInfoNeeded")]
        public Dictionary<string, string> AdditionalInfoNeeded { get; set; } = new();

        [JsonPropertyName("minAllowedAmount")]
        public double MinAllowedAmount { get; set; }

        [JsonPropertyName("maxAllowedAmount")]
        public int MaxAllowedAmount { get; set; }

        [JsonPropertyName("accreditationTime")]
        public int AccreditationTime { get; set; }

        [JsonPropertyName("financialInstitutions")]
        public FinancialInstitutions? FinancialInstitutions { get; set; }
    }

    public class FinancialInstitutions
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class Settings
    {
        [JsonPropertyName("bin")]
        public Bin? Bin { get; set; }

        [JsonPropertyName("cardNumber")]
        public CardNumber? CardNumber { get; set; }

        [JsonPropertyName("securityCode")]
        public SecurityCode? SecurityCode { get; set; }
    }

    public class SecurityCode
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("cardLocation")]
        public string CardLocation { get; set; } = string.Empty;
    }

    public class CardNumber
    {
        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("validation")]
        public string Validation { get; set; } = string.Empty;
    }

    public class Bin
    {
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;

        [JsonPropertyName("exclusionPattern")]
        public string ExclusionPattern { get; set; } = string.Empty;

        [JsonPropertyName("installmentsPattern")]
        public string InstallmentsPattern { get; set; } = string.Empty;
    }
}