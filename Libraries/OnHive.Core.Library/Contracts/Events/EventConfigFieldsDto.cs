using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Events
{
    public class EventConfigFieldsDto
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}