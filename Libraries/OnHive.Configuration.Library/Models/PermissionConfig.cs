namespace OnHive.Configuration.Library.Models
{
    public class PermissionConfig
    {
        public static readonly PermissionConfig Empty = new PermissionConfig();

        public static readonly PermissionConfig EmptyAllowUnvalidatedEmail = new PermissionConfig { AllowUnvalidatedEmail = true };

        public static PermissionConfig Create(params string[] permissions)
        {
            return new PermissionConfig
            {
                Permissions = permissions.ToList()
            };
        }

        public static PermissionConfig Create(bool allowUnvalidatedEmail, params string[] permissions)
        {
            return new PermissionConfig
            {
                Permissions = permissions.ToList(),
                AllowUnvalidatedEmail = allowUnvalidatedEmail
            };
        }

        public List<string> Permissions { get; set; } = [];

        public bool AllowUnvalidatedEmail { get; set; } = false;
    }
}