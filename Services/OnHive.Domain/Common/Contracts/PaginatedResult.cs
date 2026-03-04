using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Common
{
    public class PaginatedResult<T>
    {
        [JsonPropertyName("pageCount")]
        public long PageCount { get; set; }

        [JsonPropertyName("page")]
        public long Page { get; set; }

        [JsonPropertyName("total")]
        public long Total { get; set; }

        [JsonPropertyName("itens")]
        public List<T> Itens { get; set; } = new();

        public static PaginatedResult<T> Create(IEnumerable<T> itens, long pageCount = 0, long page = 0, long total = 0) =>
            new PaginatedResult<T> { Itens = itens.ToList(), PageCount = pageCount, Page = page, Total = total };
    }
}