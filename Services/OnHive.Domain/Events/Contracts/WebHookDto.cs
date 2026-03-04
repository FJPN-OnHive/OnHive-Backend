using OnHive.Core.Library.Enums.WebHook;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Events
{
    public class WebHookDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [MaxLength(256)]
        public string? Description { get; set; }

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("useAuthorization")]
        public bool UseAuthorization { get; set; } = false;

        [JsonPropertyName("userID")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("steps")]
        public List<WebHookStepDto> Steps { get; set; } = [];
    }

    public class WebHookStepDto
    {
        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public WebHookStepTypes Type { get; set; } = WebHookStepTypes.None;

        [JsonPropertyName("script")]
        public string? Script { get; set; }

        [JsonPropertyName("actions")]
        public List<WebHookActionDto> Actions { get; set; } = [];
    }

    public class WebHookActionDto
    {
        [JsonPropertyName("type")]
        public WebHookActionTypes Type { get; set; } = WebHookActionTypes.Replace;

        [JsonPropertyName("sourceType")]
        public WebHookFieldSourceTypes SourceType { get; set; } = WebHookFieldSourceTypes.Body;

        [JsonPropertyName("sourceField")]
        public string SourceField { get; set; } = string.Empty;

        [JsonPropertyName("sourceIndexField")]
        public string SourceIndexField { get; set; } = string.Empty;

        [JsonPropertyName("targetCollection")]
        public string TargetCollection { get; set; } = string.Empty;

        [JsonPropertyName("targetIndexField")]
        public string TargetIndexField { get; set; } = string.Empty;

        [JsonPropertyName("targetField")]
        public string TargetField { get; set; } = string.Empty;
    }
}