using MongoDB.Driver;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Tenants.Domain.Abstractions.Repositories;

namespace OnHive.Tenants.Repositories
{
    public class FeaturesRepository : MongoRepositoryBase<SystemFeatures>, IFeaturesRepository
    {
        public FeaturesRepository(MongoDBSettings settings) : base(settings, "SystemFeatures")
        {
        }

        public Task<SystemFeatures> GetAsync()
        {
            return collection
                .Find(Builders<SystemFeatures>.Filter.Empty)
                .FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<SystemFeatures>(Builders<SystemFeatures>.IndexKeys.Ascending(i => i.Hash)));
        }
    }
}