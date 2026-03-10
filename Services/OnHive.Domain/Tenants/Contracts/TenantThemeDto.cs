using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Tenants
{
    public class TenantThemeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("siteUrl")]
        public string SiteUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("catalogUrl")]
        public string CatalogUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("courseUrl")]
        public string CourseUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("termsUrl")]
        public string TermsUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("redirectCourses")]
        public string RedirectCourses { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("favicon")]
        public string Favicon { get; set; } = string.Empty;
        
        [JsonPropertyName("appleTouchIcon")]
        public string AppleTouchIcon { get; set; } = string.Empty;
        
        [JsonPropertyName("icon32x32")]
        public string Icon32x32 { get; set; } = string.Empty;
        
        [JsonPropertyName("icon16x16")]
        public string Icon16x16 { get; set; } = string.Empty;
        
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
        
        [JsonPropertyName("logo")]
        public string Logo { get; set; } = string.Empty;
        
        [JsonPropertyName("logoWhite")]
        public string LogoWhite { get; set; } = string.Empty;
        
        [JsonPropertyName("clarityId")]
        public string ClarityId { get; set; } = string.Empty;
        
        [JsonPropertyName("googleAnalyticsId")]
        public string GoogleAnalyticsId { get; set; } = string.Empty;
        
        [JsonPropertyName("googleTagManagerId")]
        public string GoogleTagManagerId { get; set; } = string.Empty;
        
        [JsonPropertyName("googleOptimizeId")]
        public string GoogleOptimizeId { get; set; } = string.Empty;

        [JsonPropertyName("instagramUrl")]
        public string InstagramUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("facebookUrl")]
        public string FacebookUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("linkedinUrl")]
        public string LinkedInUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("youtubeUrl")]
        public string YoutubeUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("tikTokUrl")]
        public string TikTokUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("pinterestUrl")]
        public string PinterestUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("discordUrl")]
        public string DiscordUrl { get; set; } = string.Empty;

        [JsonPropertyName("tokens")]
        public TenantTokensDto Tokens { get; set; } = new();
    
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }


    public class TenantTokensDto
    {
       public ColorTokenDto Primary { get; set; } = new() {
        _100 = "#fdf9c8",
        _200 = "#fcf28b",
        _300 = "#fae64f",
        _400 = "#f9d626",
        _500 = "#f3bb19",
        _600 = "#d78f08",
        _700 = "#b2660b",
        _800 = "#914f0f",
        _900 = "#442104"
       };

       public ColorTokenDto Secondary { get; set; } = new() {
        _100 = "#faebcb",
        _200 = "#f5d592",
        _300 = "#efba5a",
        _400 = "#eba134",
        _500 = "#e58625",
        _600 = "#ca6015",
        _700 = "#a84315",
        _800 = "#883418",
        _900 = "#401408"
       };

       public StatusTokenDto Status { get; set; } = new() {
        Success = "#28a745",
        Danger = "#dc3545",
        Warning = "#ffc107",
        Info = "#17a2b8"
       };

       public FontTokenDto Font { get; set; } = new() {
        FontFamily = new() {
            Primary = "Poppins",
            Secondary = "Helvetica"
        }
       };
    }

    public class ColorTokenDto
    {
        [JsonPropertyName("100")]
        public string _100 { get; set; } = string.Empty;
        [JsonPropertyName("200")]
        public string _200 { get; set; } = string.Empty;
        [JsonPropertyName("300")]
        public string _300 { get; set; } = string.Empty;
        [JsonPropertyName("400")]
        public string _400 { get; set; } = string.Empty;
        [JsonPropertyName("500")]
        public string _500 { get; set; } = string.Empty;
        [JsonPropertyName("600")]
        public string _600 { get; set; } = string.Empty;
        [JsonPropertyName("700")]
        public string _700 { get; set; } = string.Empty;
        [JsonPropertyName("800")]
        public string _800 { get; set; } = string.Empty;
        [JsonPropertyName("900")]
        public string _900 { get; set; } = string.Empty;
    }

    public class StatusTokenDto
    {
        [JsonPropertyName("success")]
        public string Success { get; set; } = string.Empty;

        [JsonPropertyName("danger")]
        public string Danger { get; set; } = string.Empty;

        [JsonPropertyName("warning")]
        public string Warning { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;
    }

    public class FontTokenDto
    {
        [JsonPropertyName("fontFamily")]
        public FontFamilyTokenDto FontFamily { get; set; } = new();
    }

    public class FontFamilyTokenDto
    {
        [JsonPropertyName("primary")]
        public string Primary { get; set; } = string.Empty;
        
        [JsonPropertyName("secondary")]
        public string Secondary { get; set; } = string.Empty;
    }
}