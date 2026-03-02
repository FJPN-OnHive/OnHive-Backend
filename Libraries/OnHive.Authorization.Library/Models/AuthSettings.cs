using System.ComponentModel.DataAnnotations;

namespace EHive.Authorization.Library.Models
{
    public class AuthSettings
    {
        [Required(ErrorMessage = "JWT Secret is required.")]
        public string? Secret { get; set; }

        public string? Audience { get; set; }

        public string? Issuer { get; set; }
    }
}