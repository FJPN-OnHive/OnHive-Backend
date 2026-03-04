using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Enums.Courses;

namespace OnHive.Core.Library.Entities.Courses
{
    public class Lesson : EntityBase
    {
        public string Code { get; set; } = string.Empty;

        public LessonTypes Type { get; set; } = LessonTypes.Lesson;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public int Order { get; set; } = 0;

        public string Thumbnail { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;

        public string VideoId { get; set; } = string.Empty;

        public string EmbeddedVideo { get; set; } = string.Empty;

        public string ArticleUrl { get; set; } = string.Empty;

        public Exam? Exam { get; set; }

        public List<string> Tags { get; set; } = new();

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<CourseStaff> Staff { get; set; } = new();

        public string Url { get; set; } = string.Empty;

        public List<Material> Materials { get; set; } = new();

        public List<string> MetaData { get; set; } = new();

        public int TotalTimeMinutes { get; set; }
    }
}