using OnHive.Core.Library.Entities.SystemParameters;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.SystemParameters.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace OnHive.SystemParameters.Repositories
{
    public class SystemParametersRepository : MongoRepositoryBase<SystemParameter>, ISystemParametersRepository
    {
        public SystemParametersRepository(MongoDBSettings settings) : base(settings, "SystemParameters")
        {
        }

        public async Task<IEnumerable<SystemParameter>> GetAllAsync()
        {
            var filter = Builders<SystemParameter>.Filter.Empty;
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<SystemParameter>> GetByGroupAsync(string group)
        {
            var filter = Builders<SystemParameter>.Filter.Eq(p => p.Group, group);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<SystemParameter>(Builders<SystemParameter>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}