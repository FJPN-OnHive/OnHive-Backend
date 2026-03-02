using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Carreiras
{
    public class Job : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime InitialDate { get; set; }

        public DateTime FinalDate { get; set; }

        public int Priority { get; set; } = 0;

        public string? ExternalLink { get; set; }
    }
}