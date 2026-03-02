namespace EHive.Core.Library.Contracts.Common
{
    public class FilterScope
    {
        public List<FilterScopeField> Fields { get; set; } = [];
    }

    public class FilterScopeField
    {
        public string Field { get; set; } = string.Empty;

        public List<string>? Values { get; set; }

        public string? MinValue { get; set; }

        public string? MaxValue { get; set; }
    }
}