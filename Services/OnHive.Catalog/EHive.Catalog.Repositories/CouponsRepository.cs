using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Core.Library.Entities.Catalog;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Catalog.Repositories
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