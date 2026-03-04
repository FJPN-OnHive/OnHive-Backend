using OnHive.Core.Library.Abstractions.Enrich;
using OnHive.Core.Library.Attributes;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Validations.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Users
{
    public class UserDto : IEnrichable
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("surname")]
        [MaxLength(256)]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        [MaxLength(256)]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("mainEmail")]
        public string MainEmail => Emails.Find(e => e.IsMain)?.Email ?? string.Empty;

        [JsonPropertyName("emails")]
        [ValidateObject]
        public List<UserEmailDto> Emails { get; set; } = new();

        [JsonPropertyName("isChangePasswordRequested")]
        public bool IsChangePasswordRequested { get; set; }

        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }

        [JsonPropertyName("isActive")]
        [Required]
        public bool IsActive { get; set; }

        [JsonPropertyName("roles")]
        [Required]
        public List<string> Roles { get; set; } = new();

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();

        [JsonPropertyName("tenant")]
        public TenantDto? Tenant { get; set; }

        [JsonPropertyName("addresses")]
        [ValidateObject(true)]
        public List<AddressDto>? Addresses { get; set; }

        [JsonPropertyName("documents")]
        public List<UserDocumentDto>? Documents { get; set; }

        [JsonPropertyName("socialName")]
        [MaxLength(256)]
        public string? SocialName { get; set; }

        [JsonPropertyName("birthDate")]
        public DateTime? BirthDate { get; set; }

        [JsonPropertyName("gender")]
        [ValidValues(typeof(GenderValidValues))]
        [Required]
        public string? Gender { get; set; }

        [JsonPropertyName("nationality")]
        [ValidValues(typeof(CountryValidValues))]
        [Required]
        public string? Nationality { get; set; }

        [JsonPropertyName("phoneNumber")]
        [MaxLength(256)]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("mobilePhoneNumber")]
        [MaxLength(256)]
        public string? MobilePhoneNumber { get; set; }

        [JsonPropertyName("isForeigner")]
        public bool IsForeigner { get; set; } = false;

        [JsonPropertyName("occupation")]
        public string? Occupation { get; set; }

        [JsonPropertyName("vId")]
        public string? VId { get; set; }

        [JsonPropertyName("customAttributes")]
        public Dictionary<string, object> CustomAttributes { get; set; } = [];

        [JsonPropertyName("tempPassword")]
        public TempPasswordDto? TempPassword { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class UserDocumentDto
    {
        [JsonPropertyName("documentNumber")]
        [MaxLength(256)]
        public string DocumentNumber { get; set; } = string.Empty;

        [JsonPropertyName("documentType")]
        [ValidValues(typeof(DocumentTypeValidValues))]
        public string DocumentType { get; set; } = string.Empty;
    }

    public class UserEmailDto
    {
        [JsonPropertyName("isMain")]
        public bool IsMain { get; set; } = false;

        [JsonPropertyName("email")]
        [MaxLength(256)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("isValidated")]
        [Required]
        public bool IsValidated { get; set; } = false;
    }

    public class TempPasswordDto
    {
        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }
    }
}