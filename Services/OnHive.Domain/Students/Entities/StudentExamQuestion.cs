using OnHive.Core.Library.Enums.Students;
using MongoDB.Bson.Serialization.Attributes;

namespace OnHive.Core.Library.Entities.Students
{
    [BsonIgnoreExtraElements]
    public class StudentExamQuestion
    {
        public string Id { get; set; } = string.Empty;

        public StudentExamQuestionState State { get; set; } = StudentExamQuestionState.Pending;

        public double Score { get; set; } = 0.0;

        public bool Correct { get; set; } = false;

        public string ResponseText { get; set; } = string.Empty;

        public List<string> ResponseOptions { get; set; } = [];
    }
}