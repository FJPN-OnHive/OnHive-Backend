namespace OnHive.Core.Library.Entities.Emails
{
    public class ComposedEmail : EntityBase
    {
        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public List<string> Attachments { get; set; } = new();

        public List<string> SendTo { get; set; } = new();

        public string From { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;

        public DateTime SendDate { get; set; }
    }
}