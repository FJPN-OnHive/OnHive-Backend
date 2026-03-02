using EHive.Core.Library.Enums.Messages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Messages
{
    public class MessageDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [JsonPropertyName("channelCode")]
        public string ChannelCode { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public MessageFromDto From { get; set; } = new();

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        [JsonPropertyName("status")]
        public MessageStatus Status { get; set; }

        [JsonPropertyName("messageDate")]
        public DateTime MessageDate { get; set; }

        [JsonPropertyName("expireDate")]
        public DateTime ExpireDate { get; set; }
    }

    public class MessageFromDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;
    }
}