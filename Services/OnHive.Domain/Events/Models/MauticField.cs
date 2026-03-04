using System.Text.Json.Serialization;

namespace OnHive.Events.Domain.Models
{
    public class MauticField
    {
        [JsonPropertyName("isPublished")]
        public bool IsPublished { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("group")]
        public string? Group { get; set; }

        [JsonPropertyName("defaultValue")]
        public string? DefaultValue { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("properties")]
        public MauticFieldProperty? Properties { get; set; }
    }

    public class MauticFieldProperty
    {
        [JsonPropertyName("no")]
        public string? No { get; set; }

        [JsonPropertyName("yes")]
        public string? Yes { get; set; }

        [JsonPropertyName("list")]
        public List<MauticProperty>? List { get; set; }
    }

    public class MauticProperty
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}