using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Tenants
{
    public class TenantSetupDto
    {
        [Required]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [Required]
        [JsonPropertyName("cnpj")]
        public string? CNPJ { get; set; }

        [Required]
        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [Required]
        [JsonPropertyName("adminUserName")]
        public string? AdminUserName { get; set; }

        [Required]
        [JsonPropertyName("adminUserEmail")]
        public string? AdminUserEmail { get; set; }

        [Required]
        [JsonPropertyName("adminUserPassword")]
        public string? AdminUserPassword { get; set; }
    }
}