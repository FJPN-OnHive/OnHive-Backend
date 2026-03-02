using MongoDB.Driver;
using EHive.Core.Library.Entities.Payments;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Payments.Domain.Abstractions.Repositories;

namespace EHive.Payments.Repositories
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