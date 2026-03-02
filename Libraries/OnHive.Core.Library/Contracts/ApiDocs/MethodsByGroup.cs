namespace EHive.Core.Library.Contracts.ApiDocs
{
    public class MethodsByGroup
    {
        public MethodsByGroup(ApiDocItemDto service, string group, List<ApiDocItemMethodDto> methods)
        {
            Service = service;
            Group = group;
            Methods = methods;
        }

        public ApiDocItemDto Service { get; set; }

        public string Group { get; set; }

        public List<ApiDocItemMethodDto> Methods { get; set; } = new();

        public bool Visible { get; set; } = false;
    }
}