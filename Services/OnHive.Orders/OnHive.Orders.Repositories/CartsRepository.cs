using OnHive.Core.Library.Entities.Orders;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Orders.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace OnHive.Carts.Repositories
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