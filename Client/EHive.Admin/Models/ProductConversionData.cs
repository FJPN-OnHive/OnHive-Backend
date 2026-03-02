using System.Text.Json.Serialization;

namespace EHive.Admin.Models
{
    public class ProductConversionData
    {
        [JsonPropertyName("conversao")]
        public string Conversao { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }
}