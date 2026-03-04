using System.Text.Json.Nodes;

namespace OnHive.Core.Library.Entities.Enrichments
{
    public class Enrichment : EntityBase
    {
        public string EntityId { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public string CustomAttributes { get; set; } = string.Empty;
    }
}