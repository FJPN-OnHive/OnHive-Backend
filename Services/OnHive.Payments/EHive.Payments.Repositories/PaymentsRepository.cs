using MongoDB.Driver;
using EHive.Core.Library.Entities.Payments;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Payments.Domain.Abstractions.Repositories;

namespace EHive.Payments.Repositories
{
    public class PaymentsRepository : MongoRepositoryBase<Payment>, IPaymentsRepository
    {
        public PaymentsRepository(MongoDBSettings settings) : base(settings, "Payments")
        {
        }

        public async Task<Payment> GetByOrderIdAsync(string orderId, string tenantId)
        {
            var filter = Builders<Payment>.Filter.Eq(p => p.OrderId, orderId)
                & Builders<Payment>.Filter.Eq(p => p.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Payment>> GetByProviderAsync(string providerKey, string tenantId)
        {
            var filter = Builders<Payment>.Filter.Eq(p => p.ProviderKey, providerKey)
               & Builders<Payment>.Filter.Eq(p => p.TenantId, tenantId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<Payment>> GetByUserIdAsync(string userId, string tenantId)
        {
            var filter = Builders<Payment>.Filter.Eq(p => p.UserId, userId)
               & Builders<Payment>.Filter.Eq(p => p.TenantId, tenantId);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Payment>(Builders<Payment>.IndexKeys.Ascending(i => i.UserId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Payment>(Builders<Payment>.IndexKeys.Ascending(i => i.OrderId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Payment>(Builders<Payment>.IndexKeys.Ascending(i => i.ProviderKey)));
        }
    }
}