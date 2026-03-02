using MongoDB.Driver;
using EHive.Core.Library.Entities.Redirects;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Redirects.Domain.Abstractions.Repositories;
using EHive.Core.Library.Entities.Tenants;
using System.IO;
using System.Runtime.InteropServices;

namespace EHive.Redirects.Repositories
{
    public class RedirectRepository : MongoRepositoryBase<Redirect>, IRedirectRepository
    {
        public RedirectRepository(MongoDBSettings settings) : base(settings, "Redirects")
        {
        }

        public async Task<bool> DeleteAsync(string redirectId, string id)
        {
            var filter = Builders<Redirect>.Filter.Eq(r => r.Id, redirectId);
            var result = await collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public Task<List<Redirect>> GetAllActive(string tenantId)
        {
            var filter = Builders<Redirect>.Filter.Eq(r => r.TenantId, tenantId)
                            & Builders<Redirect>.Filter.Eq(r => r.IsActive, true);
            return collection.Find(filter).ToListAsync();
        }

        public Task<Redirect> GetByPathAsync(string tenantId, string path)
        {
            var filter = Builders<Redirect>.Filter.Eq(r => r.TenantId, tenantId)
                            & Builders<Redirect>.Filter.Eq(r => r.Path, path);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Redirect>(Builders<Redirect>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}