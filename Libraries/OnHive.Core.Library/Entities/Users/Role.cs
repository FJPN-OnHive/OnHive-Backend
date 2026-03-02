namespace EHive.Core.Library.Entities.Users
{
    public class Role : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();

        public bool IsAdmin { get; set; }
    }
}