using OnHive.Core.Library.Enums.Events;

namespace OnHive.Core.Library.Entities.Events
{
    public class AutomationWebHook
    {
        public string Url { get; set; } = string.Empty;

        public AutomationWebHookMethod Method { get; set; } = AutomationWebHookMethod.GET;

        public string Body { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/json";

        public Dictionary<string, string> Headers { get; set; } = [];
    }
}