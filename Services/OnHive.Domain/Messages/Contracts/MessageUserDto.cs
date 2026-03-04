using OnHive.Core.Library.Enums.Messages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Messages
{
    public class MessageUserDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("messageId")]
        [MaxLength(256)]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public MessageStatus Status { get; set; } = MessageStatus.New;

        [JsonPropertyName("readDate")]
        public DateTime ReadDate { get; set; }

        [JsonPropertyName("messageDate")]
        public DateTime MessageDate { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;
    }
}