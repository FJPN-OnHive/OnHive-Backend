using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.ApiDocs
{
    public class ApiDocItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("service")]
        public string Service { get; set; } = string.Empty;

        [JsonPropertyName("methods")]
        public List<ApiDocItemMethodDto> Methods { get; set; } = new();
    }

    public class ApiDocItemMethodDto
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;
    }
}