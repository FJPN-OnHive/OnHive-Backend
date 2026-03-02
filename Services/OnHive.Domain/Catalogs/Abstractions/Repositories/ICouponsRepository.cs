using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Catalog;

namespace EHive.Catalog.Domain.Abstractions.Repositories
{
    public interface ICouponsRepository : IRepositoryBase<Coupon>
    {
        Task<Coupon> GetByKey(string tenantId, string key);
    }
}