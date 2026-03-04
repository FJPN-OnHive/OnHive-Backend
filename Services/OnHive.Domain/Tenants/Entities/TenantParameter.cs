namespace OnHive.Core.Library.Entities.Tenants
{
    public class TenantParameter : EntityBase
    {
        public string Key { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}