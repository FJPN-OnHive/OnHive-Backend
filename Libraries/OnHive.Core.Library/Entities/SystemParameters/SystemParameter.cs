namespace EHive.Core.Library.Entities.SystemParameters
{
    public class SystemParameter : EntityBase
    {
        public string Group { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}