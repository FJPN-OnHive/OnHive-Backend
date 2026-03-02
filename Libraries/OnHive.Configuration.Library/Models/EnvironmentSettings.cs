using System.ComponentModel.DataAnnotations;

namespace EHive.Configuration.Library.Models
{
    public class EnvironmentSettings
    {
        public string EnvironmentType { get; set; } = "staging";
    }
}