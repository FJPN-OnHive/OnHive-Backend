using MongoDB.Bson.Serialization.Attributes;

namespace EHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class StudentDiscipline
    {
        public string Id { get; set; } = string.Empty;

        public string VId { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        public List<StudentLesson> Lessons { get; set; } = new();
    }
}