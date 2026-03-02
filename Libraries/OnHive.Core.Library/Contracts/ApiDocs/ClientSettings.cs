using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.ApiDocs
{
    public class ClientSettings
    {
        [JsonPropertyName("apiGateway")]
        public string ApiGateway { get; set; } = string.Empty;
    }
}