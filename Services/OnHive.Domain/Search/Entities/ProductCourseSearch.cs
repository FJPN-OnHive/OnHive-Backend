using EHive.Core.Library.Entities.Catalog;

namespace EHive.Core.Library.Entities.Search
{
    public class ProductCourseSearch : EntityBase
    {
        public string CourseId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageAltText { get; set; }
        public string? ItemUrl { get; set; }
        public int TotalTimeMinutes { get; set; } = 0;
        public double Rate { get; set; } = 0;
        public int DifficultLevel { get; set; }
        public string Slug { get; set; } = string.Empty;
        public double FullPrice { get; set; } = 0;
        public double LowPrice { get; set; } = 0;
        public int Sales { get; set; } = 0;
        public string? Url { get; set; }
        public string? Category { get; set; }
        public List<string>? Categories { get; set; }
        public string? Tags { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime SnapshotDate { get; set; }
        public List<ProductPrice> Prices { get; set; } = new();

        public string? ExternalUrl { get; set; }
    }
}