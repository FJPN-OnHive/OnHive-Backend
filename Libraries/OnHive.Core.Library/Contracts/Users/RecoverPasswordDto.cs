using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Users
{
    public class RecoverPasswordDto
    {
        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        [Required]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("newPassword")]
        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [JsonPropertyName("query")]
        public string Query => ToString() ?? string.Empty;

        public override string? ToString()
        {
            return $"?tenantId={TenantId}&code={Code}&newPassword={NewPassword}";
        }
    }
}