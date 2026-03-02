using EHive.Core.Library.Enums.Posts;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Posts
{
    public class BlogPostDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        [MaxLength(512)]
        [Required]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(1024)]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        [Required]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("authorId")]
        public string? AuthorId { get; set; }

        [JsonPropertyName("author")]
        [MaxLength(256)]
        public string? Author { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("coverImage")]
        public string? CoverImage { get; set; }

        [JsonPropertyName("coverImageAltText")]
        public string? CoverImageAltText { get; set; }

        [JsonPropertyName("status")]
        public PostStatus Status { get; set; } = PostStatus.Draft;

        [JsonPropertyName("visibility")]
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;

        [JsonPropertyName("publishDate")]
        public DateTime? PublishDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("requiredCourses")]
        public List<string> RequiredCourses { get; set; } = [];

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        [JsonPropertyName("metaData")]
        public PostMetadataDto MetaData { get; set; } = new();

        [JsonPropertyName("likes")]
        public int? Likes { get; set; } = 0;

        [JsonPropertyName("likedByCurrentUser")]
        public bool? LikedByCurrentUser { get; set; } = false;

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("breadcrumbTitle")]
        public string? BreadcrumbTitle { get; set; }

        [JsonPropertyName("canonicalUrl")]
        public string? CanonicalUrl { get; set; }

        [JsonPropertyName("alternativeSlugs")]
        public List<string>? AlternativeSlugs { get; set; }
    }
}