namespace OnHive.Domains.Events.Models
{
    public class EventRegisterServiceSettings
    {
        public string Origin { get; set; } = string.Empty;

        public List<string> DefaultTags { get; set; } = [];

        public Dictionary<string, string> DefaultFields { get; set; } = [];
    }
}