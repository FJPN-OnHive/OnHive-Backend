using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Courses
{
    public class Discipline : EntityBase
    {
        public string Code { get; set; } = string.Empty;

        public int Order { get; set; } = 1;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Thumbnail { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public List<string> Lessons { get; set; } = new();

        public List<string> Exams { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        public List<string> MetaData { get; set; } = new();
    }
}