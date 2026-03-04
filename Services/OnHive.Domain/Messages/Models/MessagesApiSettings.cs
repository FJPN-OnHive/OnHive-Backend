namespace OnHive.Messages.Domain.Models
{
    public class MessagesApiSettings
    {
        public string? MessagesAdminPermission { get; set; } = "messages_admin";

        public double MessageSimilarityLimit { get; set; } = 0.8;

        public int DuplicatedMessageTimeCheckHours { get; set; } = 1;
    }
}