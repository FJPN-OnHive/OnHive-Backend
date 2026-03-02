using EHive.Core.Library.Attributes;
using EHive.Core.Library.Enums.Users;
using EHive.Core.Library.Validations.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Users
{
    public class UserProfileCompleteDto
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

        [JsonPropertyName("name")]
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("surname")]
        [MaxLength(256)]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("socialName")]
        [MaxLength(256)]
        public string? SocialName { get; set; }

        [JsonPropertyName("gender")]
        [ValidValues(typeof(GenderValidValues))]
        [Required]
        public string? Gender { get; set; }

        [JsonPropertyName("title")]
        [MaxLength(256)]
        public string? Title { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("mainEmail")]
        public string? MainEmail { get; set; }

        [JsonPropertyName("profilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        [JsonPropertyName("coverPictureUrl")]
        public string? CoverPictureUrl { get; set; }

        [JsonPropertyName("publicEmail")]
        public bool PublicEmail { get; set; } = false;
    }
}