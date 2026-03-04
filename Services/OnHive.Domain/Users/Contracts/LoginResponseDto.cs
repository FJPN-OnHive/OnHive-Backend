using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Login
{
    public class LoginResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public UserDto User { get; set; } = new();

        public static bool TryParse(string input, out LoginResponseDto? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<LoginResponseDto>(input);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public override string? ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}