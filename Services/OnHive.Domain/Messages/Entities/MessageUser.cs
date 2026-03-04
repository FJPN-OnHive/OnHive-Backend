using OnHive.Core.Library.Enums.Messages;

namespace OnHive.Core.Library.Entities.Messages
{
    public class MessageUser : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string MessageId { get; set; } = string.Empty;

        public MessageStatus Status { get; set; } = MessageStatus.New;

        public DateTime ReadDate { get; set; }

        public DateTime MessageDate { get; set; }

        public string From { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }
}