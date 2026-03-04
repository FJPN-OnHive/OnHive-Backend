using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Login
{
    public class LoginRedirectResponseDto
    {
        [JsonPropertyName("loginCode")]
        public string LoginCode { get; set; } = string.Empty;

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; } = string.Empty;
    }
}