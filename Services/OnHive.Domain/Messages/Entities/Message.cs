using EHive.Core.Library.Enums.Messages;

namespace EHive.Core.Library.Entities.Messages
{
    public class Message : EntityBase
    {
        public string ChannelId { get; set; } = string.Empty;

        public string ChannelCode { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public MessageFrom From { get; set; } = new();

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = [];

        public MessageStatus Status { get; set; }

        public DateTime MessageDate { get; set; }

        public DateTime ExpireDate { get; set; }
    }

    public class MessageFrom
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}