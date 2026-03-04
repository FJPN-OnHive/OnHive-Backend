using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Entities.Tenants
{
    public class SystemFeatures : EntityBase
    {
        [JsonPropertyName("features")]
        public List<Feature> Features { get; set; } = new List<Feature>();

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
    }

    public class Feature
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("service")]
        public string Service { get; set; } = string.Empty;
    }
}