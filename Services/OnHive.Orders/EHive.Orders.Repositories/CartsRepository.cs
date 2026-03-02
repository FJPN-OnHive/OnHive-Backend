using EHive.Core.Library.Entities.Orders;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Orders.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace EHive.Carts.Repositories
{
    public class CartsRepository : MongoRepositoryBase<Cart>, ICartsRepository
    {
        public CartsRepository(MongoDBSettings settings) : base(settings, "Carts")
        {
        }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(string id, string tenantId)
        {
            var filter = Builders<Cart>.Filter.Eq(e => e.TenantId, tenantId)
                            & Builders<Cart>.Filter.Eq(e => e.UserId, id);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Cart>(Builders<Cart>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}