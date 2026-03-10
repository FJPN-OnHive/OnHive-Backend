using MongoDB.Bson.Serialization.Attributes;

namespace OnHive.Core.Library.Entities.Tenants
{
    [BsonIgnoreExtraElements]
    public class TenantTheme : EntityBase
    {
        public string SiteUrl { get; set; } = string.Empty;
        
        public string CatalogUrl { get; set; } = string.Empty;
        
        public string CourseUrl { get; set; } = string.Empty;
        
        public string TermsUrl { get; set; } = string.Empty;
        
        public string RedirectCourses { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Favicon { get; set; } = string.Empty;
        
        public string AppleTouchIcon { get; set; } = string.Empty;
        
        public string Icon32x32 { get; set; } = string.Empty;
        
        public string Icon16x16 { get; set; } = string.Empty;
        
        public string Icon { get; set; } = string.Empty;
        
        public string Logo { get; set; } = string.Empty;
        
        public string LogoWhite { get; set; } = string.Empty;

        public string ClarityId { get; set; } = string.Empty;
        
        public string GoogleAnalyticsId { get; set; } = string.Empty;
        
        public string GoogleTagManagerId { get; set; } = string.Empty;
        
        public string GoogleOptimizeId { get; set; } = string.Empty;

        public string InstagramUrl { get; set; } = string.Empty;
        
        public string FacebookUrl { get; set; } = string.Empty;
        
        public string LinkedInUrl { get; set; } = string.Empty;
        
        public string YoutubeUrl { get; set; } = string.Empty;
        
        public string TikTokUrl { get; set; } = string.Empty;
        
        public string PinterestUrl { get; set; } = string.Empty;

        public string DiscordUrl { get; set; } = string.Empty;

        public TenantTokens Tokens { get; set; } = new();
    }

    public class TenantTokens
    {
       public ColorToken Primary { get; set; } = new() {
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

       public ColorToken Secondary { get; set; } = new() {
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

       public StatusToken Status { get; set; } = new() {
        Success = "#28a745",
        Danger = "#dc3545",
        Warning = "#ffc107",
        Info = "#17a2b8"
       };

       public FontToken Font { get; set; } = new() {
        FontFamily = new() {
            Primary = "Poppins",
            Secondary = "Helvetica"
        }
       };
    }

    public class ColorToken
    {
        public string _100 { get; set; } = string.Empty;
        public string _200 { get; set; } = string.Empty;
        public string _300 { get; set; } = string.Empty;
        public string _400 { get; set; } = string.Empty;
        public string _500 { get; set; } = string.Empty;
        public string _600 { get; set; } = string.Empty;
        public string _700 { get; set; } = string.Empty;
        public string _800 { get; set; } = string.Empty;
        public string _900 { get; set; } = string.Empty;
    }

    public class StatusToken
    {
        public string Success { get; set; } = string.Empty;

        public string Danger { get; set; } = string.Empty;

        public string Warning { get; set; } = string.Empty;

        public string Info { get; set; } = string.Empty;
    }

    public class FontToken
    {
        public FontFamilyToken FontFamily { get; set; } = new();
    }

    public class FontFamilyToken
    {
        public string Primary { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;
    }
}