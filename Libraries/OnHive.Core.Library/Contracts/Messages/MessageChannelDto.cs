using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Messages
{
    public class MessageChannelDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("messagesExpirationDays")]
        public int MessagesExpirationDays { get; set; } = 0;

        [JsonPropertyName("sendEmail")]
        public bool SendEmail { get; set; } = true;

        [JsonPropertyName("emailTemplateCode")]
        public string EmailTemplateCode { get; set; } = string.Empty;

        [JsonPropertyName("usersIds")]
        public List<string> UsersIds { get; set; } = [];

        [JsonPropertyName("usersGroupIds")]
        public List<string> UsersGroupIds { get; set; } = [];
    }
}