using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Catalog;

namespace EHive.Catalog.Domain.Abstractions.Repositories
{
    public interface IUserCouponsRepository : IRepositoryBase<UserCoupon>
    {
        Task<IEnumerable<UserCoupon>> GetByCouponId(string tenantId, string couponId);

        Task<IEnumerable<UserCoupon>> GetByUserId(string tenantId, string userId);

        Task<IEnumerable<UserCoupon>> GetByUserIdAndCouponId(string tenantId, string userId, string couponId);

        Task<IEnumerable<UserCoupon>> GetByOrderId(string tenantId, string orderId);

        Task<IEnumerable<UserCoupon>> GetByProductId(string tenantId, string productId);
    }
}