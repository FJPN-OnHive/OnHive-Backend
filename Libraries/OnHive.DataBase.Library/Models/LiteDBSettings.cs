using System.ComponentModel.DataAnnotations;

namespace EHive.Database.Library.Models
{
    public class LiteDBSettings
    {
        [Required(ErrorMessage = "LiteDb Database Path required")]
        public string? DataBasePath { get; set; }
    }
}