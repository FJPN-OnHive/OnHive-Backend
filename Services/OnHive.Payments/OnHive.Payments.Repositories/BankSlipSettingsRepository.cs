using MongoDB.Driver;
using OnHive.Core.Library.Entities.Payments;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Payments.Domain.Abstractions.Repositories;

namespace OnHive.Payments.Repositories
{
    public class BankSlipSettingsRepository : MongoRepositoryBase<BankSlipSettings>, IBankSlipSettingsRepository
    {
        public BankSlipSettingsRepository(MongoDBSettings settings) : base(settings, "BankSlipSettings")
        {
        }

        public Task<BankSlipSettings> GetByProviderAsync(string bankSlipProviderKey)
        {
            var filter = Builders<BankSlipSettings>.Filter.Eq(i => i.Provider, bankSlipProviderKey);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<BankSlipSettings>(Builders<BankSlipSettings>.IndexKeys.Ascending(i => i.Provider)));
        }
    }
}