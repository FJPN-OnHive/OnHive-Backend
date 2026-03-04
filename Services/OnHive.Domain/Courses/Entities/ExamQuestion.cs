using OnHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Entities.Courses
{
    public class ExamQuestion
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int Order { get; set; } = 0;

        public string AuxText { get; set; } = string.Empty;

        public QuestionTypes Type { get; set; } = QuestionTypes.SingleChoice;

        public List<QuestionOption> Options { get; set; } = new();

        public double Value { get; set; } = 0.0;

        public bool Optional { get; set; } = false;
    }

    public class QuestionOption
    {
        public string Id { get; set; } = string.Empty;

        public int Order { get; set; } = 0;

        public string Letter { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;
    }
}