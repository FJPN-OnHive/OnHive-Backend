using Microsoft.OpenApi;
using System.Text.Json;

namespace OnHive.Configuration.Library.Models
{
    public class PermissionDocs : IOpenApiExtension
    {
        public List<PermissionDoc> Permissions { get; set; } = new();

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteOptionalCollection("permissions", Permissions, (w, o) => w.WriteRaw(JsonSerializer.Serialize(o)));
        }
    }

    public class PermissionDoc : IOpenApiElement
    {
        public string Endpoint { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
    }
}