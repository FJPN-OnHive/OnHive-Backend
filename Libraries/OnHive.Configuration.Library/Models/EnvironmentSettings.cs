using System.ComponentModel.DataAnnotations;

namespace OnHive.Configuration.Library.Models
{
    public class EnvironmentSettings
    {
        public string EnvironmentType { get; set; } = "staging";
    }
}