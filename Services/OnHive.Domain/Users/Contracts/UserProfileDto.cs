using OnHive.Core.Library.Enums.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Users
{
    public class UserProfileDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public ProfileTypes Type { get; set; } = ProfileTypes.Student;

        [JsonPropertyName("userId")]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        [MaxLength(256)]
        public string? Title { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("profilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        [JsonPropertyName("coverPictureUrl")]
        public string? CoverPictureUrl { get; set; }

        [JsonPropertyName("publicEmail")]
        public bool PublicEmail { get; set; } = false;
    }
}