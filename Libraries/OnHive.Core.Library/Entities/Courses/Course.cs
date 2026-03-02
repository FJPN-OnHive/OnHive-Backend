using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Courses
{
    public class Course : EntityBase
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? Category { get; set; }

        public List<string>? Categories { get; set; }

        public List<string> Tags { get; set; } = new();

        public List<string> Disciplines { get; set; } = new();

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int TotalTimeMinutes { get; set; }

        public List<CourseStaff> Staff { get; set; } = new();

        public string? Url { get; set; }

        public string? Thumbnail { get; set; }

        public string? ImageUrl { get; set; }

        public List<string> MetaData { get; set; } = new();

        public List<string> RelatedCourses { get; set; } = [];

        public float Rate { get; set; } = 0;

        public string Slug { get; set; } = string.Empty;

        public int DifficultLevel { get; set; } = 1;

        public List<string> Requirements { get; set; } = [];

        public int Duration { get; set; } = 0;

        public DateTime? LaunchDate { get; set; }

        public double ApprovalAverage { get; set; } = 7.0;

        public int LessonsCount { get; set; } = 0;

        public int MaterialsCount { get; set; } = 0;

        public bool HasCertificate { get; set; } = false;

        public string CertificateId { get; set; } = string.Empty;
    }
}