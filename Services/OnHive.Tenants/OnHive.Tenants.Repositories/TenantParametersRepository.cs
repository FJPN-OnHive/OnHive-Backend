using MongoDB.Driver;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Tenants.Domain.Abstractions.Repositories;

namespace OnHive.Tenants.Repositories
{
    public class TenantParametersRepository : MongoRepositoryBase<TenantParameter>, ITenantParametersRepository
    {
        public TenantParametersRepository(MongoDBSettings settings) : base(settings, "TenantParameters")
        {
        }

        public async Task<IEnumerable<TenantParameter>> GetByGroup(string group, string tenantId)
        {
            var filter = Builders<TenantParameter>.Filter.Eq(e => e.TenantId, tenantId)
                & Builders<TenantParameter>.Filter.Eq(e => e.Group, group);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<TenantParameter> GetByKey(string group, string key, string tenantId)
        {
            var filter = Builders<TenantParameter>.Filter.Eq(e => e.TenantId, tenantId)
               & Builders<TenantParameter>.Filter.Eq(e => e.Group, group)
               & Builders<TenantParameter>.Filter.Eq(e => e.Key, key);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<TenantParameter>(Builders<TenantParameter>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<TenantParameter>(Builders<TenantParameter>.IndexKeys.Ascending(i => i.Group)));
            collection.Indexes.CreateOne(new CreateIndexModel<TenantParameter>(Builders<TenantParameter>.IndexKeys.Ascending(i => i.Key)));
        }
    }
}