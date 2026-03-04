using OnHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class MaterialDto
    {
        [JsonPropertyName("type")]
        public MaterialTypes Type { get; set; } = MaterialTypes.Other;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("metaData")]
        public List<string> MetaData { get; set; } = new();
    }
}