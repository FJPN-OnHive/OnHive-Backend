namespace EHive.Core.Library.Entities.Events
{
    public class EventConfig : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public List<EventConfigFields> Fields { get; set; } = [];

        public bool IsPersistent { get; set; } = false;
    }
}