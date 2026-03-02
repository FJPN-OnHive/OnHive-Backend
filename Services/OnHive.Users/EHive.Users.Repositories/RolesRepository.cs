using MongoDB.Driver;
using EHive.Core.Library.Entities.Users;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Users.Domain.Abstractions.Repositories;

namespace EHive.Users.Repositories
{
    public class RolesRepository : MongoRepositoryBase<Role>, IRolesRepository
    {
        public RolesRepository(MongoDBSettings settings) : base(settings, "Roles")
        {
        }

        public async Task<Role?> GetByNameAsync(string roleName, string tenantId)
        {
            var filter = Builders<Role>.Filter.Eq(r => r.Name, roleName)
                & Builders<Role>.Filter.Eq(r => r.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Role>(Builders<Role>.IndexKeys.Ascending(i => i.Name)));
            collection.Indexes.CreateOne(new CreateIndexModel<Role>(Builders<Role>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}