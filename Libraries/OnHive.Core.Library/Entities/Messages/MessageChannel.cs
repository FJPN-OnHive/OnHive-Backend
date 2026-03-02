namespace EHive.Core.Library.Entities.Messages
{
    public class MessageChannel : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public int MessagesExpirationDays { get; set; } = 0;

        public bool SendEmail { get; set; } = true;

        public string EmailTemplateCode { get; set; } = string.Empty;

        public List<string> UsersIds { get; set; } = [];

        public List<string> UsersGroupIds { get; set; } = [];
    }
}