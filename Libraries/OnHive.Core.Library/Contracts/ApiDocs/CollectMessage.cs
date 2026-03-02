using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.ApiDocs
{
    public class CollectMessage
    {
        [JsonPropertyName("environment")]
        public string Environment { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}