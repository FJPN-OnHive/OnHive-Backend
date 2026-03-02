using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Catalog;

namespace EHive.Catalog.Domain.Abstractions.Repositories
{
    public interface IProductsRepository : IRepositoryBase<Product>
    {
        Task<Product> GetBySku(string tenantId, string sku);

        Task<Product> GetBySlug(string tenantId, string slug);

        Task<Product> GetByAlternativeSlug(string slug, string tenantId);

        Task<IEnumerable<Product>> GetByName(string tenantId, string name);

        Task<List<Product>> GetAllActive(string tenantId);

        Task<FilterScope> GetFilterDataAsync(string tenantId);

        Task<PaginatedResult<Product>> GetByFilterActiveAsync(RequestFilter filter, string tenantId);

        Task<List<Product>> GetByIdsAsync(List<string> productIds);

        Task<List<Product>> GetByItemIdsAsync(List<string> itensIds);
    }
}