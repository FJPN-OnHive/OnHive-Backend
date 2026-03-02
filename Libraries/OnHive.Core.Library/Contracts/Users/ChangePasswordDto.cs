using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Users
{
    public class ChangePasswordDto
    {
        [JsonPropertyName("userId")]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("oldPasswordHash")]
        [Required]
        public string OldPasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("newPassword")]
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}