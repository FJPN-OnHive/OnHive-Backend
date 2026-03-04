using MongoDB.Driver;
using OnHive.Core.Library.Entities.Payments;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Payments.Domain.Abstractions.Repositories;

namespace OnHive.Payments.Repositories
{
    public class BankSlipNumberControlRepository : MongoRepositoryBase<BankSlipNumberControl>, IBankSlipNumberControlRepository
    {
        public BankSlipNumberControlRepository(MongoDBSettings settings) : base(settings, "BankSlipNumberControl")
        {
        }

        public BankSlipNumberControl GetByProvider(string providerKey)
        {
            var filter = Builders<BankSlipNumberControl>.Filter.Eq(i => i.ProviderKey, providerKey);
            return collection.Find(filter).FirstOrDefault();
        }

        public async Task<int> GetNextAsync(string providerKey, string orderId, string paymentId)
        {
            var filter = Builders<BankSlipNumberControl>.Filter.Eq(i => i.ProviderKey, providerKey);
            var result = await collection.UpdateOneAsync(filter, Builders<BankSlipNumberControl>.Update
                                .Inc(i => i.LastNumber, 1)
                                .Set(i => i.LastOrderId, orderId)
                                .Set(i => i.LastPaymentId, paymentId));
            if (result.ModifiedCount == 0)
            {
                await collection.InsertOneAsync(new BankSlipNumberControl
                {
                    ProviderKey = providerKey,
                    LastPaymentId = paymentId,
                    LastNumber = 1,
                    LastOrderId = orderId
                });
                return 1;
            }
            var updated = await collection.Find(filter).FirstOrDefaultAsync();
            return updated.LastNumber;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<BankSlipNumberControl>(Builders<BankSlipNumberControl>.IndexKeys.Ascending(i => i.ProviderKey)));
        }
    }
}