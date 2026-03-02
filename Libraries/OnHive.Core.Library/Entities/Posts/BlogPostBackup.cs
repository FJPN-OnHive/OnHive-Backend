using EHive.Core.Library.Enums.Posts;

namespace EHive.Core.Library.Entities.Posts
{
    public class BlogPostBackup : EntityBase
    {
        public string PostId { get; set; } = string.Empty;

        public DateTime SnapShotDate { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? AuthorId { get; set; }

        public string? Author { get; set; }

        public string? Thumbnail { get; set; }

        public string? CoverImage { get; set; }

        public string? CoverImageAltText { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Draft;

        public PostVisibility Visibility { get; set; } = PostVisibility.Public;

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        public List<string> RequiredCourses { get; set; } = [];

        public string Slug { get; set; } = string.Empty;

        public List<string> Categories { get; set; } = [];

        public List<string> Tags { get; set; } = [];

        public PostMetadata MetaData { get; set; } = new();

        public List<BlogPostLike> Likes { get; set; } = [];

        public string? BreadcrumbTitle { get; set; }

        public string? CanonicalUrl { get; set; }

        public List<string>? AlternativeSlugs { get; set; }
    }
}