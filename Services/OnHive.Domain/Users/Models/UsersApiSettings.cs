namespace OnHive.Users.Domain.Models
{
    public class UsersApiSettings
    {
        public string DefaultRole { get; set; } = string.Empty;

        public JwtAuth? JwtAuth { get; set; } = new JwtAuth();

        public GoogleAuth? GoogleAuth { get; set; }

        public bool EnableBasicLogin { get; set; } = false;

        public string UserAdminPermission { get; set; } = "users_admin";

        public string UserEmailValidationTemplate { get; set; } = "EMAIL_VALIDATION";

        public string UserEmailValidationUrl { get; set; } = string.Empty;

        public string UserEmailValidationService { get; set; } = string.Empty;

        public string PasswordRecoverTemplate { get; set; } = "PASSWORD_RECOVERY";

        public string PasswordRecoverUrl { get; set; } = string.Empty;

        public string PasswordRecoverService { get; set; } = string.Empty;

        public int ValidationCodesDurationMinutes { get; set; } = 60;

        public int ValidationCodesSize { get; set; } = 6;

        public string PasswordPattern { get; set; } = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\\^$@#!&+\\-=\\[\\]{}()%*_\\\\;':\"|\\,\\.\\/?~])(?![^\\^$@#!&+\\-=\\[\\]{}()%*\\\\_;':\"|\\,\\.\\/?~a-zA-Z0-9])(^\\S{8,30}$)";

        public List<AppAccessSettings> AppsAccessSettings { get; set; } = new List<AppAccessSettings> {
            new AppAccessSettings
            {
                AppName = "LMS",
                AppParameterId = "LMS_DOMAIN",
                AppPermissions = "lms_access"
            },
            new AppAccessSettings
            {
                AppName = "ACCOUNT",
                AppParameterId = "ACCOUNT_DOMAIN",
                AppPermissions = "account_access"
            },
            new AppAccessSettings
            {
                AppName = "CMS",
                AppParameterId = "CMS_DOMAIN",
                AppPermissions = "cms_access"
            },
            new AppAccessSettings
            {
                AppName = "ADMIN",
                AppParameterId = "ADMIN_DOMAIN",
                AppPermissions = "admin_access"
            }
        };

        public int MinAge { get; set; } = 12;
    }

    public class JwtAuth
    {
        public string? SecretKey { get; set; } = string.Empty;

        public string ClientKey { get; set; } = string.Empty;

        public string? Issuer { get; set; } = "OnHive.com.br";

        public string? Audience { get; set; } = "OnHive.com.br";

        public int ExpirationTimeMinutes { get; set; } = 90;

        public int RenewTimeHours { get; set; } = 48;
    }

    public class GoogleAuth
    {
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;
    }

    public class AppAccessSettings
    {
        public string AppName { get; set; } = string.Empty;

        public string AppParameterId { get; set; } = string.Empty;

        public string AppPermissions { get; set; } = string.Empty;
    }
}