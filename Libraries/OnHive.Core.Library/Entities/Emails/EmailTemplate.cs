namespace EHive.Core.Library.Entities.Emails
{
    public class EmailTemplate : EntityBase
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }
}