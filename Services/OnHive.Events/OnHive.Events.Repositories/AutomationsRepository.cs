using MongoDB.Driver;
using OnHive.Core.Library.Entities.Events;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Events.Domain.Abstractions.Repositories;

namespace OnHive.Events.Repositories
{
    public class AutomationsRepository : MongoRepositoryBase<Automation>, IAutomationsRepository
    {
        public AutomationsRepository(MongoDBSettings settings) : base(settings, "Automations")
        {
        }

        public Task<List<Automation>> GetByKey(string tenantId, string key)
        {
            var filter = Builders<Automation>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<Automation>.Filter.Eq(x => x.EventKey, key);
            return collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Automation>(Builders<Automation>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Automation>(Builders<Automation>.IndexKeys.Ascending(i => i.EventKey)));
        }
    }
}