using MongoDB.Driver;
using EHive.Core.Library.Entities.Events;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Events.Domain.Abstractions.Repositories;

namespace EHive.Events.Repositories
{
    public class EventsRepository : MongoRepositoryBase<EventRegister>, IEventsRepository
    {
        public EventsRepository(MongoDBSettings settings) : base(settings, "Events")
        {
        }

        public Task<List<EventRegister>> GetByKey(string tenantId, string key)
        {
            var filter = Builders<EventRegister>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventRegister>.Filter.Eq(x => x.Key, key);
            return collection.Find(filter).ToListAsync();
        }

        public Task<List<EventRegister>> GetByOrigin(string tenantId, string origin)
        {
            var filter = Builders<EventRegister>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventRegister>.Filter.Eq(x => x.Origin, origin);
            return collection.Find(filter).ToListAsync();
        }

        public Task<List<EventRegister>> GetFilter(string tenantId, DateTime initial, DateTime final, string? origin, string? key)
        {
            var filter = Builders<EventRegister>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventRegister>.Filter.Gte(x => x.Date, initial)
                & Builders<EventRegister>.Filter.Lte(x => x.Date, final);

            if (!string.IsNullOrEmpty(origin))
            {
                filter &= Builders<EventRegister>.Filter.Eq(x => x.Origin, origin);
            }

            if (!string.IsNullOrEmpty(key))
            {
                filter &= Builders<EventRegister>.Filter.Eq(x => x.Key, key);
            }
            return collection.Find(filter).ToListAsync();
        }

        public Task<List<EventRegister>> GetPeriod(string tenantId, DateTime initial, DateTime final)
        {
            var filter = Builders<EventRegister>.Filter.Eq(x => x.TenantId, tenantId)
                & Builders<EventRegister>.Filter.Gte(x => x.Date, initial)
                & Builders<EventRegister>.Filter.Lte(x => x.Date, final);
            return collection.Find(filter).ToListAsync();
        }

        public async Task<long> RemoveNonPersistentOlderThanAsync(DateTime referenceDate)
        {
            var filter = Builders<EventRegister>.Filter.Eq(x => x.IsPersistent, false)
                         & Builders<EventRegister>.Filter.Lt(x => x.Date, referenceDate);
            var result = await collection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<EventRegister>(Builders<EventRegister>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventRegister>(Builders<EventRegister>.IndexKeys.Ascending(i => i.Key)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventRegister>(Builders<EventRegister>.IndexKeys.Ascending(i => i.IsPersistent)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventRegister>(Builders<EventRegister>.IndexKeys.Ascending(i => i.Origin)));
            collection.Indexes.CreateOne(new CreateIndexModel<EventRegister>(Builders<EventRegister>.IndexKeys.Ascending(i => i.Date)));
        }
    }
}