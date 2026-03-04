namespace OnHive.Core.Library.Entities.Posts
{
    public class BlogPostLike
    {
        public string UserId { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}