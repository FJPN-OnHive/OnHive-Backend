namespace EHive.Core.Library.Entities.Users
{
    public class DefaultPermissions : EntityBase
    {
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public string Hash { get; set; } = string.Empty;
    }

    public class Permission
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}