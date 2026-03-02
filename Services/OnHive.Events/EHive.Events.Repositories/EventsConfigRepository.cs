using MongoDB.Driver;
using EHive.Core.Library.Entities.Events;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Events.Domain.Abstractions.Repositories;

namespace EHive.Events.Repositories
{
    public class EventsConfigRepository : MongoRepositoryBase<EventConfig>, IEventsConfigRepository
    {
        public EventsConfigRepository(MongoDBSettings settings) : base(settings, "EventsConfig")
        {
        }

        public Task<EventConfig> GetByKeyAndOrigin(string tenantId, string key, string origin)
        {
            var filter = Builders<EventConfig>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventConfig>.Filter.Eq(x => x.Key, key)
                & Builders<EventConfig>.Filter.Eq(x => x.Origin, origin);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        public Task<List<EventConfig>> GetByOrigin(string tenantId, string origin)
        {
            var filter = Builders<EventConfig>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventConfig>.Filter.Eq(x => x.Origin, origin);
            return collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<EventConfig>(Builders<EventConfig>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventConfig>(Builders<EventConfig>.IndexKeys.Ascending(i => i.Origin)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventConfig>(Builders<EventConfig>.IndexKeys.Ascending(i => i.Key)));
        }
    }
}