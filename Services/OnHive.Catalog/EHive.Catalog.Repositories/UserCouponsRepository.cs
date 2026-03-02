using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Core.Library.Entities.Catalog;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Catalog.Repositories
{
    public class UserCouponsRepository : MongoRepositoryBase<UserCoupon>, IUserCouponsRepository
    {
        public UserCouponsRepository(MongoDBSettings settings) : base(settings, "UserCoupons")
        {
        }

        public async Task<IEnumerable<UserCoupon>> GetByCouponId(string tenantId, string couponId)
        {
            var filter = Builders<UserCoupon>.Filter.Eq(p => p.TenantId, tenantId)
                           & Builders<UserCoupon>.Filter.Eq(p => p.CouponId, couponId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<UserCoupon>> GetByOrderId(string tenantId, string orderId)
        {
            var filter = Builders<UserCoupon>.Filter.Eq(p => p.TenantId, tenantId)
                           & Builders<UserCoupon>.Filter.Eq(p => p.OrderId, orderId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<UserCoupon>> GetByProductId(string tenantId, string productId)
        {
            var filter = Builders<UserCoupon>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<UserCoupon>.Filter.Eq(p => p.ProductId, productId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<UserCoupon>> GetByUserId(string tenantId, string userId)
        {
            var filter = Builders<UserCoupon>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<UserCoupon>.Filter.Eq(p => p.UserId, userId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<UserCoupon>> GetByUserIdAndCouponId(string tenantId, string userId, string couponId)
        {
            var filter = Builders<UserCoupon>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<UserCoupon>.Filter.Eq(p => p.UserId, userId)
                            & Builders<UserCoupon>.Filter.Eq(p => p.CouponId, couponId);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<UserCoupon>(Builders<UserCoupon>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<UserCoupon>(Builders<UserCoupon>.IndexKeys.Ascending(i => i.CouponId)));
            collection.Indexes.CreateOne(new CreateIndexModel<UserCoupon>(Builders<UserCoupon>.IndexKeys.Ascending(i => i.UserId)));
        }
    }
}