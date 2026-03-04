using OnHive.Configuration.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Configuration;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace OnHive.Configuration.Repositories
{
    public class ConfigurationRepository : MongoRepositoryBase<ConfigItem>, IConfigurationRepository
    {
        public ConfigurationRepository(MongoDBSettings settings) : base(settings, "Configs")
        {
        }

        public async Task<ConfigItem> GetByKeyAsync(string key)
        {
            var filter = Builders<ConfigItem>.Filter.Eq(e => e.Key, key);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<ConfigItem>(Builders<ConfigItem>.IndexKeys.Ascending(i => i.Key)));
        }
    }
}