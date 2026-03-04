namespace OnHive.Core.Library.Entities.Search
{
    public class SearchResult : EntityBase
    {
        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SourceId { get; set; } = string.Empty;

        public string? SourceImageUrl { get; set; }

        public string? SourceImageAltText { get; set; }

        public string? SourceSlug { get; set; }

        public string? SourceUrl { get; set; }

        public string? ExternalUrl { get; set; }
    }
}