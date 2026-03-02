using EHive.Core.Library.Enums.Events;

namespace EHive.Core.Library.Entities.Events
{
    public class Automation : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string EventKey { get; set; } = string.Empty;

        public AutomationType Type { get; set; }

        public AutomationWebHook? WebHook { get; set; }

        public AutomationEmail? Email { get; set; }

        public List<AutomationCondition> Conditions { get; set; } = [];
    }
}