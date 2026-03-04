using MongoDB.Driver;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Orders.Domain.Abstractions.Repositories;

namespace OnHive.Orders.Repositories
{
    public class OrdersRepository : MongoRepositoryBase<Order>, IOrdersRepository
    {
        public OrdersRepository(MongoDBSettings settings) : base(settings, "Orders")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Order>(Builders<Order>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}
