using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Videos
{
    public class VideoUploadDto
    {
        [JsonPropertyName("videoRegistry")]
        public VideoDto? VideoRegistry { get; set; }

        [JsonPropertyName("uploadUrl")]
        public string UploadUrl { get; set; } = string.Empty;
    }
}