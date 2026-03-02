namespace EHive.Core.Library.Entities.Events
{
    public class EventRegister : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public Dictionary<string, string> Fields { get; set; } = [];

        public List<string> Tags { get; set; } = [];

        public bool IsPersistent { get; set; } = false;
    }
}