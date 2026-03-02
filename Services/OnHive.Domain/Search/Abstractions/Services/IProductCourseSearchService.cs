using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Search;

namespace EHive.Search.Domain.Abstractions.Services
{
    public interface IProductCourseSearchService
    {
        Task<PaginatedResult<ProductCourseSearchDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<FilterScope> GetFilterScopeAsync(string tenantId);

        Task<int> TriggerGathering(bool full);
    }
}