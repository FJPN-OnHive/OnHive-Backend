using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Users
{
    public class RoleDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("permissions")]
        [Required]
        public List<string> Permissions { get; set; } = new();

        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }
    }
}