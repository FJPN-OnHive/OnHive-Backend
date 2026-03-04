namespace OnHive.Core.Library.Entities.Users
{
    public class DefaultRoles : EntityBase
    {
        public List<DefaultRole> Roles { get; set; } = new List<DefaultRole>();

        public string Hash { get; set; } = string.Empty;
    }

    public class DefaultRole
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();

        public bool IsAdmin { get; set; }
    }
}