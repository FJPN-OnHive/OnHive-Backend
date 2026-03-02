using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Login
{
    public class LoginDto
    {
        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        [Required]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("passwordHash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("remindMe")]
        [Required]
        public bool RemindMe { get; set; } = false;

        [JsonPropertyName("appName")]
        [Required]
        public string AppName { get; set; } = string.Empty;

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; } = string.Empty;
    }
}