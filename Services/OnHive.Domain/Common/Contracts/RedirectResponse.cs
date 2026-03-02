using EHive.Core.Library.Enums.Common;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Common
{
    public class RedirectResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("redirectCode")]
        public string RedirectCode { get; set; } = string.Empty;

        public Response<RedirectResponse> GetResponse()
        {
            return new Response<RedirectResponse>
            {
                Code = ResponseCode.Redirect,
                Message = "Redirect",
                Payload = this
            };
        }
    }
}