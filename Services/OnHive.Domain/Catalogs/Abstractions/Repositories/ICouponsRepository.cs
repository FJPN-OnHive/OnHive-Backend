using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Catalog;

namespace OnHive.Catalog.Domain.Abstractions.Repositories
{
    public interface ICouponsRepository : IRepositoryBase<Coupon>
    {
        Task<Coupon> GetByKey(string tenantId, string key);
    }
}