using EHive.Core.Library.Enums.Events;

namespace EHive.Core.Library.Entities.Events
{
    public class AutomationEmail
    {
        public string To { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }
}