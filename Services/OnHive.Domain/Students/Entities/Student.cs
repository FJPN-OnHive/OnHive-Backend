using MongoDB.Bson.Serialization.Attributes;

namespace EHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class Student : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public List<StudentCourse> Courses { get; set; } = new();
    }
}