using OnHive.Catalog.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace OnHive.Catalog.Repositories
{
    public class CouponsRepository : MongoRepositoryBase<Coupon>, ICouponsRepository
    {
        public CouponsRepository(MongoDBSettings settings) : base(settings, "Coupons")
        {
        }

        public async Task<Coupon> GetByKey(string tenantId, string key)
        {
            var filter = Builders<Coupon>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<Coupon>.Filter.Eq(p => p.Key, key);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Coupon>(Builders<Coupon>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Coupon>(Builders<Coupon>.IndexKeys.Ascending(i => i.Key)));
        }
    }
}