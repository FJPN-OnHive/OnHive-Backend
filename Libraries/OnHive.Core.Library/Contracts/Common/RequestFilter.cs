using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Common
{
    public class RequestFilter
    {
        [JsonPropertyName("filter")]
        public List<FilterField> Filter { get; set; } = new();

        [JsonPropertyName("andFilter")]
        public List<FilterField> AndFilter { get; set; } = new();

        [JsonPropertyName("orFilter")]
        public List<FilterField> OrFilter { get; set; } = new();

        [JsonPropertyName("sort")]
        public List<FieldSort> Sort { get; set; } = new();

        [JsonPropertyName("pageLimit")]
        public int PageLimit { get; set; } = 0;

        [JsonPropertyName("page")]
        public int Page { get; set; } = 0;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "AND";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";

        public override string? ToString()
        {
            var result = new List<string>();
            if (Page > 0)
            {
                result.Add($"page={Page}");
            }
            if (PageLimit > 0)
            {
                result.Add($"page_limit={PageLimit}");
            }
            if (Filter.Any())
            {
                result.Add($"filter={string.Join(';', Filter.Select(f => f.ToString()))}");
            }
            if (AndFilter.Any())
            {
                result.Add($"and_filter={string.Join(';', AndFilter.Select(f => f.ToString()))}");
            }
            if (OrFilter.Any())
            {
                result.Add($"or_filter={string.Join(';', OrFilter.Select(f => f.ToString()))}");
            }
            result.Add($"type={Type}");
            if (!string.IsNullOrEmpty(Text))
            {
                result.Add($"text={Text}");
            }
            if (Sort.Any())
            {
                result.Add($"sort={string.Join(';', Sort.Select(s => s.ToString()))}");
            }

            return string.Join('&', result);
        }
    }

    public class FilterField
    {
        [JsonIgnore]
        public static List<string> ValidOperators => new List<string> { "reg", "gt", "lt", "gte", "lte", "eq", "ne", "in", "nin", "btw", "btwx" };

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("operator")]
        public string Operator { get; set; } = "eq";

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("valueVariation")]
        public string ValueVariantion { get; set; } = string.Empty;

        public static bool TryParse(string input, out FilterField? filterField)
        {
            filterField = null;
            try
            {
                var fieldParts = input.Split(':');
                if (fieldParts.Length != 3 && fieldParts.Length != 4) return false;
                if (!ValidOperators.Any(o => o.Equals(fieldParts[1], StringComparison.InvariantCultureIgnoreCase))) return false;
                if (fieldParts.Length != 4)
                {
                    filterField = new FilterField { Field = fieldParts[0], Operator = fieldParts[1], Value = fieldParts[2] };
                }
                else
                {
                    filterField = new FilterField { Field = fieldParts[0], Operator = fieldParts[1], Value = fieldParts[2], ValueVariantion = fieldParts[3] };
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string? ToString()
        {
            if (string.IsNullOrEmpty(ValueVariantion))
            {
                return $"{Field}:{Operator}:{Value}";
            }
            else
            {
                return $"{Field}:{Operator}:{Value}:{ValueVariantion}";
            }
        }
    }

    public class FieldSort
    {
        [JsonIgnore]
        public static List<string> ValidOrders => new List<string> { "asc", "desc", "a", "d" };

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public string Order { get; set; } = "ASC";

        public static bool TryParse(string input, out FieldSort? FieldSort)
        {
            FieldSort = null;
            try
            {
                var fieldParts = input.Split(':');
                if (fieldParts.Length != 2) return false;
                if (!ValidOrders.Any(o => o.Equals(fieldParts[1], StringComparison.InvariantCultureIgnoreCase))) return false;
                FieldSort = new FieldSort { Field = fieldParts[0], Order = fieldParts[1] };
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string? ToString()
        {
            return $"{Field}:{Order}";
        }
    }
}