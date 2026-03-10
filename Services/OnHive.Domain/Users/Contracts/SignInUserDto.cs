using OnHive.Core.Library.Attributes;
using OnHive.Core.Library.Validations.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Users
{
    public class SignInUserDto
    {
        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        [MaxLength(256)]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        [MaxLength(256)]
        [MinLength(5)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        [MinLength(2)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("surname")]
        [MaxLength(256)]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("gender")]
        [ValidValues(typeof(GenderValidValues))]
        [Required]
        public string? Gender { get; set; }

        [JsonPropertyName("document")]
        public UserDocumentDto Document { get; set; } = new();

        [JsonPropertyName("phoneNumber")]
        [MaxLength(256)]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        [MaxLength(256)]
        [Required]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("isForeigner")]
        [Required]
        public bool IsForeigner { get; set; } = false;

        [JsonPropertyName("occupation")]
        public string Occupation { get; set; } = string.Empty;
    }
}