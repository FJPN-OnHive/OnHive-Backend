using MongoDB.Driver;
using EHive.Core.Library.Entities.Dict;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Dict.Domain.Abstractions.Repositories;

namespace EHive.Dict.Repositories
{
    public class DictRepository : MongoRepositoryBase<ValueRegistry>, IDictRepository
    {
        public DictRepository(MongoDBSettings settings) : base(settings, "Dict")
        {
        }

        public Task<ValueRegistry> GetByGroupAndKeyAsync(string tenantId, string group, string key)
        {
            var filter = Builders<ValueRegistry>.Filter.Eq(i => i.Group, group)
                & Builders<ValueRegistry>.Filter.Eq(i => i.Key, key)
                & Builders<ValueRegistry>.Filter.Eq(i => i.TenantId, tenantId);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetGroupsAsync(string tenantId)
        {
            var filter = Builders<ValueRegistry>.Filter.Eq(i => i.TenantId, tenantId);
            return (await collection.Find(filter).ToListAsync()).Select(i => i.Group).Distinct().ToList();
        }

        public async Task<List<string>> GetKeysAsync(string tenantId, string group)
        {
            var filter = Builders<ValueRegistry>.Filter.Eq(i => i.TenantId, tenantId) &
                            Builders<ValueRegistry>.Filter.Eq(i => i.Group, group);
            return (await collection.Find(filter).ToListAsync()).Select(i => i.Key).Distinct().ToList();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<ValueRegistry>(Builders<ValueRegistry>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<ValueRegistry>(Builders<ValueRegistry>.IndexKeys.Ascending(i => i.Group)));
            collection.Indexes.CreateOne(new CreateIndexModel<ValueRegistry>(Builders<ValueRegistry>.IndexKeys.Ascending(i => i.Key)));
        }
    }
}