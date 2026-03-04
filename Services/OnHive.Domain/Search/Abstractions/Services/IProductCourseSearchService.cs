using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Search;

namespace OnHive.Search.Domain.Abstractions.Services
{
    public interface IProductCourseSearchService
    {
        Task<PaginatedResult<ProductCourseSearchDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<FilterScope> GetFilterScopeAsync(string tenantId);

        Task<int> TriggerGathering(bool full);
    }
}