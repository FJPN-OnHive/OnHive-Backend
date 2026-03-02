using System.ComponentModel.DataAnnotations;

namespace EHive.Tenants.Domain.Models
{
    public class TenantsApiSettings
    {
        [Required(ErrorMessage = "Tenants Admin setting Required")]
        public string? TenantsAdminPermission { get; set; } = "tenants_admin";
    }
}