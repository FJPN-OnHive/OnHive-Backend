using OnHive.Core.Library.Contracts.Common;

namespace OnHive.Admin.Helpers;

public static class PaginationHelper
{
    public static int CalculateTotalPages(long pageCount, long total, int pageLimit, int returnedCount)
    {
        var computedPageCount = pageCount > 0
            ? pageCount
            : (pageLimit > 0 ? (long)Math.Ceiling(total / (double)pageLimit) : 0);

        return computedPageCount > 0
            ? (int)computedPageCount
            : (returnedCount > 0 ? 1 : 0);
    }

    public static void ApplyOrSearchFilter(RequestFilter filter, string searchTerm, params string[] fields)
    {
        ArgumentNullException.ThrowIfNull(filter);

        searchTerm = searchTerm?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filter.OrFilter = [];
            return;
        }

        filter.OrFilter = [.. fields.Select(field => new FilterField
        {
            Field = field,
            Operator = "reg",
            Value = searchTerm
        })];
    }

    public static int ValidatePageNumber(int requestedPage, int totalPages) =>
        requestedPage < 1 ? 1 :
        totalPages > 0 && requestedPage > totalPages ? totalPages :
        requestedPage;
}
