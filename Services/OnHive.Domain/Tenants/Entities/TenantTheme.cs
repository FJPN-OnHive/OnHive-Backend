namespace OnHive.Core.Library.Entities.Tenants
{
    public class TenantTheme : EntityBase
    {
        public bool IsBaseStyle { get; set; } = false;

        public string Domain { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Logo { get; set; } = string.Empty;

        public string? CompanyName { get; set; } = string.Empty;

        public string? Banner { get; set; } = string.Empty;

        public string? PrimaryColor { get; set; } = string.Empty;

        public string? SecondaryColor { get; set; } = string.Empty;

        public string? PrimaryAccentColor { get; set; } = string.Empty;

        public string? SecondaryAccentColor { get; set; } = string.Empty;

        public string? PrimaryBackgroundColor { get; set; } = string.Empty;

        public string? SecondaryBackgroundColor { get; set; } = string.Empty;

        public string? PrimaryFontFamily { get; set; } = string.Empty;

        public string? SecondaryFontFamily { get; set; } = string.Empty;

        public Dictionary<string, string>? GeneralStyles { get; set; } = [];

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}