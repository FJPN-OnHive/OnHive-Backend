using MongoDB.Driver;
using EHive.Core.Library.Entities.Tenants;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Tenants.Domain.Abstractions.Repositories;

namespace EHive.Tenants.Repositories
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