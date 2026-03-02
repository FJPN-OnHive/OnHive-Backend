using EHive.Core.Library.Entities.Events;
using EHive.Core.Library.Enums.Events;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Events
{
    public class AutomationDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("eventKey")]
        public string EventKey { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public AutomationType Type { get; set; }

        [JsonPropertyName("webHook")]
        public AutomationWebHookDto? WebHook { get; set; }

        [JsonPropertyName("email")]
        public AutomationEmailDto? Email { get; set; }

        [JsonPropertyName("conditions")]
        public List<AutomationConditionDto> Conditions { get; set; } = [];
    }
}