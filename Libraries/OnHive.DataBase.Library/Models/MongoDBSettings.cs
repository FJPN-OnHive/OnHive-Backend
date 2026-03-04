using System.ComponentModel.DataAnnotations;

namespace OnHive.Database.Library.Models
{
    public class MongoDBSettings
    {
        [Required(ErrorMessage = "Mongo Connection string required")]
        public string? ConnectionString { get; set; }

        [Required(ErrorMessage = "Mongo database name required")]
        public string? DataBase { get; set; }
    }
}