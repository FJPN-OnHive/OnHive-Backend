using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Posts
{
    public class PostMetadataDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}