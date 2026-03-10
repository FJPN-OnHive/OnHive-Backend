using MongoDB.Driver;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Tenants.Domain.Abstractions.Repositories;

namespace OnHive.Tenants.Repositories
{
    public class TenantThemesRepository : MongoRepositoryBase<TenantTheme>, ITenantThemesRepository
    {
        public TenantThemesRepository(MongoDBSettings settings) : base(settings, "TenantThemes")
        {
        }


        public async Task<TenantTheme> GetByTenant(string tenantId)
        {
            var filter = Builders<TenantTheme>.Filter.Eq(e => e.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<TenantTheme>(Builders<TenantTheme>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}