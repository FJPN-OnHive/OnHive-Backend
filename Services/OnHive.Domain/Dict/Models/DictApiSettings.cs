using System.ComponentModel.DataAnnotations;

namespace OnHive.Dict.Domain.Models
{
    public class DictApiSettings
    {
        public string? DictAdminPermission { get; set; } = "dict_admin";
    }
}