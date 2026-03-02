using System.Text.Json.Serialization;

public class MauticContact
{
    [JsonPropertyName("firstname")]
    public string? Firstname { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("conversion")]
    public string? Conversion { get; set; }

    [JsonPropertyName("formid")]
    public string? FormId { get; set; }

    [JsonPropertyName("formname")]
    public string? FormName { get; set; }

    [JsonPropertyName("termos_de_aceite")]
    public string? TermosDeAceite { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}