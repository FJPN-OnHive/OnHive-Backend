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

        public async Task<List<TenantTheme>> GetByDomain(string domain, string tenantId)
        {
            var filter = Builders<TenantTheme>.Filter.Eq(e => e.TenantId, tenantId)
                & Builders<TenantTheme>.Filter.Eq(e => e.Domain, domain);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<TenantTheme> GetCurrentByDomain(string domain, string tenantId)
        {
            var filter = Builders<TenantTheme>.Filter.Eq(e => e.TenantId, tenantId)
               & Builders<TenantTheme>.Filter.Eq(e => e.Domain, domain)
               & Builders<TenantTheme>.Filter.Lte(e => e.StartDate, DateTime.UtcNow)
               & Builders<TenantTheme>.Filter.Gte(e => e.EndDate, DateTime.UtcNow);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<TenantTheme>(Builders<TenantTheme>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<TenantTheme>(Builders<TenantTheme>.IndexKeys.Ascending(i => i.Domain)));
            collection.Indexes.CreateOne(new CreateIndexModel<TenantTheme>(Builders<TenantTheme>.IndexKeys.Ascending(i => i.StartDate)));
            collection.Indexes.CreateOne(new CreateIndexModel<TenantTheme>(Builders<TenantTheme>.IndexKeys.Ascending(i => i.EndDate)));
        }
    }
}