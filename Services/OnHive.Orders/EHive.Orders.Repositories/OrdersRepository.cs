using MongoDB.Driver;
using EHive.Core.Library.Entities.Orders;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Orders.Domain.Abstractions.Repositories;

namespace EHive.Orders.Repositories
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
