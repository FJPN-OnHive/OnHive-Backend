namespace EHive.Core.Library.Entities.Students
{
    public class StudentAnnotation
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public int Position { get; set; } = 0;

        public DateTime TimeStamp { get; set; }
    }
}