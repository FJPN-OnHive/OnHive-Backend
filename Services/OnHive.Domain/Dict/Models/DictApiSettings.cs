using System.ComponentModel.DataAnnotations;

namespace EHive.Dict.Domain.Models
{
    public class DictApiSettings
    {
        public string? DictAdminPermission { get; set; } = "dict_admin";
    }
}