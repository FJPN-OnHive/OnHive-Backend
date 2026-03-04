namespace OnHive.Core.Library.Entities.Dict
{
    public class ValueRegistry : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = [];
    }
}