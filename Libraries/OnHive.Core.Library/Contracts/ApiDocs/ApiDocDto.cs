using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.ApiDocs
{
    public class ApiDocDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("environment")]
        public string Environment { get; set; } = string.Empty;

        [JsonPropertyName("service")]
        public string Service { get; set; } = string.Empty;

        [JsonPropertyName("openApiVersion")]
        public string OpenApiVersion { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public InfoDto Info { get; set; } = new();

        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = string.Empty;

        [JsonPropertyName("endpoints")]
        public List<EndpointDto> Endpoints { get; set; } = new();

        [JsonPropertyName("schemas")]
        public List<SchemaDto> Schemas { get; set; } = new();

        [JsonPropertyName("originalFile")]
        public string OriginalFile { get; set; } = string.Empty;

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
    }

    public class SchemaDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("properties")]
        public List<SchemaPropertyDto> Properties { get; set; } = new();

        [JsonPropertyName("additionalProperties")]
        public bool AdditionalProperties { get; set; } = false;

        [JsonPropertyName("textRepresentation")]
        public string TextRepresentation { get; set; } = string.Empty;
    }

    public class SchemaPropertyDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("maxLength")]
        public int MaxLength { get; set; } = 0;

        [JsonPropertyName("ref")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("nullable")]
        public bool Nullable { get; set; } = false;

        [JsonPropertyName("required")]
        public bool Required { get; set; } = true;
    }

    public class EndpointDto
    {
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public List<ParameterDto> Parameters { get; set; } = new();

        [JsonPropertyName("responses")]
        public List<ResponseDto> Responses { get; set; } = new();

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("requireAuthentication")]
        public bool RequireAuthentication { get; set; } = false;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();

        [JsonPropertyName("allowUnvalidatedEmail")]
        public bool AllowUnvalidatedEmail { get; set; } = false;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }

    public class ResponseDto
    {
        [JsonPropertyName("httpCode")]
        public string HttpCode { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("isReference")]
        public bool IsReference { get; set; } = false;
    }

    public class ParameterDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty;

        [JsonPropertyName("required")]
        public bool Required { get; set; } = true;

        [JsonPropertyName("style")]
        public string Style { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("isReference")]
        public bool IsReference { get; set; } = false;
    }

    public class ContactDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class InfoDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("contact")]
        public ContactDto Contact { get; set; } = new();

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}