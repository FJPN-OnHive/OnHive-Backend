using EHive.Core.Library.Enums.Events;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Events
{
    public class AutomationConditionDto
    {
        [JsonPropertyName("type")]
        public AutomationConditionType Type { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("typeCode")]
        public int TypeCode { get => (int)Type; set => Type = (AutomationConditionType)value; }
    }
}