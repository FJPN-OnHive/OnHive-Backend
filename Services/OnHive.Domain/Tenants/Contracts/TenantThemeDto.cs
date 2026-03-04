using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Tenants
{
    public class TenantThemeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("isBaseStyle")]
        public bool IsBaseStyle { get; set; } = false;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("logo")]
        public string? Logo { get; set; } = string.Empty;

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("banner")]
        public string? Banner { get; set; } = string.Empty;

        [JsonPropertyName("primaryColor")]
        public string? PrimaryColor { get; set; } = string.Empty;

        [JsonPropertyName("secondaryColor")]
        public string? SecondaryColor { get; set; } = string.Empty;

        [JsonPropertyName("primaryAccentColor")]
        public string? PrimaryAccentColor { get; set; } = string.Empty;

        [JsonPropertyName("secondaryAccentColor")]
        public string? SecondaryAccentColor { get; set; } = string.Empty;

        [JsonPropertyName("primaryBackgroundColor")]
        public string? PrimaryBackgroundColor { get; set; } = string.Empty;

        [JsonPropertyName("secondaryBackgroundColor")]
        public string? SecondaryBackgroundColor { get; set; } = string.Empty;

        [JsonPropertyName("primaryFontFamily")]
        public string? PrimaryFontFamily { get; set; } = string.Empty;

        [JsonPropertyName("secondaryFontFamily")]
        public string? SecondaryFontFamily { get; set; } = string.Empty;

        [JsonPropertyName("generalStyles")]
        public Dictionary<string, string>? GeneralStyles { get; set; } = [];

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}