using EHive.Core.Library.Enums.Courses;
using MongoDB.Bson.Serialization.Attributes;

namespace EHive.Core.Library.Entities.Courses
{
    [BsonIgnoreExtraElements]
    public class Exam : EntityBase
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public List<ExamQuestion> Questions { get; set; } = new();

        public List<string> RequiredLessons { get; set; } = new();

        public int Order { get; set; } = 0;

        public double TotalScore { get; set; } = 0.0;

        public double RequiredScore { get; set; } = 0.0;

        public List<string> Tags { get; set; } = new();

        public string Url { get; set; } = string.Empty;

        public List<string> MetaData { get; set; } = new();

        public int MaxRetries { get; set; } = 1;
    }
}