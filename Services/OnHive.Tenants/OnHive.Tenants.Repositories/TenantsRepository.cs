using OnHive.Core.Library.Entities.Tenants;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace OnHive.Tenants.Repositories
{
    public class TenantsRepository : MongoRepositoryBase<Tenant>, ITenantsRepository
    {
        public TenantsRepository(MongoDBSettings settings) : base(settings, "Tenants")
        {
        }

        public Task<List<Tenant>> GetAllAsync()
        {
            return collection.Find(Builders<Tenant>.Filter.Empty).ToListAsync();
        }

        public Task<Tenant> GetByDomainAsync(string domain)
        {
            var filter = Builders<Tenant>.Filter.Eq(e => e.Domain, domain);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        public Task<Tenant> GetBySlugAsync(string slug)
        {
            var filter = Builders<Tenant>.Filter.Eq(e => e.Slug, slug);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Tenant>(Builders<Tenant>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}