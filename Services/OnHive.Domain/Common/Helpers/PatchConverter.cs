using OnHive.Core.Library.Entities;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace OnHive.Core.Library.Helpers
{
    public static class PatchConverter
    {
        public static List<string> CommonIgnoreFields => ["id", "tenantId", "createdBy", "createdAt", "updatedBy", "updatedAt"];

        public static string GetId(this JsonDocument patchJson)
        {
            return patchJson.RootElement.GetProperty("id").GetString() ?? string.Empty;
        }

        public static string GetTenantId(this JsonDocument patchJson)
        {
            return patchJson.RootElement.GetProperty("tenantId").GetString() ?? string.Empty;
        }

        public static T PatchEntity<T>(this JsonDocument patchJson, T current) where T : EntityBase, new()
        {
            return PatchEntity<T>(patchJson, current, CommonIgnoreFields);
        }

        public static T PatchEntity<T>(this JsonDocument patchJson, T current, List<string> ignoreFields) where T : EntityBase, new()
        {
            return (T)patchJson.PatchEntity(typeof(T), current, ignoreFields);
        }

        public static object PatchEntity(this JsonDocument patchJson, Type type, object current)
        {
            return PatchEntity(patchJson, type, current, CommonIgnoreFields);
        }

        public static object PatchEntity(this JsonDocument patchJson, Type type, object current, List<string> ignoreFields)
        {
            var result = current;
            foreach (var property in patchJson.RootElement.EnumerateObject())
            {
                try
                {
                    if (ignoreFields.Exists(i => property.Name.Equals(i, StringComparison.OrdinalIgnoreCase))) continue;
                    var propertyInfo = type.GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo == null) continue;
                    var value = GetValue(property, propertyInfo, propertyInfo.GetValue(result));
                    propertyInfo.SetValue(result, value, null);
                }
                catch (Exception)
                {
                    //Continue
                }
            }
            return result;
        }

        public static object? GetValue(JsonProperty property, PropertyInfo propertyInfo, object current)
        {
            object value;
            switch (propertyInfo.PropertyType.Name)
            {
                case "String":
                    value = property.Value.GetString();
                    break;

                case "Boolean":
                    value = property.Value.GetBoolean();
                    break;

                case "Int32":
                    value = property.Value.GetInt32();
                    break;

                case "Int64":
                    value = property.Value.GetInt64();
                    break;

                case "Float":
                case "Double":
                    value = property.Value.GetDouble();
                    break;

                case "DateTime":
                    value = property.Value.GetDateTime();
                    break;

                default:
                    if (propertyInfo.PropertyType.Name.StartsWith("List"))
                    {
                        value = property.Value.Deserialize(propertyInfo.PropertyType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    else if (propertyInfo.PropertyType.BaseType.Name.Equals("enum", StringComparison.OrdinalIgnoreCase))
                    {
                        value = Enum.Parse(propertyInfo.PropertyType, property.Value.GetInt64().ToString());
                    }
                    else
                    {
                        value = JsonDocument.Parse(property.Value.GetRawText()).PatchEntity(propertyInfo.PropertyType, current);
                    }
                    break;
            }

            return value;
        }
    }
}