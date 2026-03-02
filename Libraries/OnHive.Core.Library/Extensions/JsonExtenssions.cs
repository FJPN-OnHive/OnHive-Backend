using System.Text.Json;

namespace EHive.Core.Library.Extensions
{
    public static class JsonExtensions
    {
        public static JsonElement GetElementByPath(this JsonDocument document, string path)
        {
            return GetElementByPath(document, JsonPath.Parse(path));
        }

        public static JsonElement GetElementByPath(this JsonElement element, string path)
        {
            return GetElementByPath(element, JsonPath.Parse(path));
        }

        public static JsonElement GetElementByPath(this JsonDocument document, JsonPath jsonPath)
        {
            return GetElementByPath(document.RootElement, jsonPath);
        }

        public static JsonElement GetElementByPath(this JsonElement currentElement, JsonPath jsonPath)
        {
            var element = currentElement.Clone();
            foreach (var jsonPathElement in jsonPath.Elements)
            {
                if (jsonPathElement.IsArray)
                {
                    element = element.GetProperty(jsonPathElement.Token)[jsonPathElement.Index];
                }
                else
                {
                    element = element.GetProperty(jsonPathElement.Token);
                }
            }
            return element;
        }
    }

    public class JsonPath
    {
        private JsonPath()
        { }

        public string Path { get; set; } = string.Empty;

        public List<JsonPathElement> Elements { get; set; } = [];

        public static JsonPath Parse(string path)
        {
            var result = new JsonPath();
            result.Path = path;

            var elements = path.Split('.');

            foreach (var element in elements)
            {
                var jsonPathElement = new JsonPathElement();

                if (element.Contains("[") && element.Contains("]"))
                {
                    jsonPathElement.IsArray = true;

                    var token = element.Substring(0, element.IndexOf("["));
                    var index = element.Substring(element.IndexOf("[") + 1, element.IndexOf("]") - element.IndexOf("[") - 1);

                    jsonPathElement.Token = token;
                    jsonPathElement.Index = int.Parse(index);
                }
                else
                {
                    jsonPathElement.Token = element;
                }

                result.Elements.Add(jsonPathElement);
            }

            return result;
        }
    }

    public class JsonPathElement
    {
        public bool IsArray { get; set; } = false;

        public string Token { get; set; } = string.Empty;

        public int Index { get; set; } = 0;
    }
}