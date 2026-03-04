using MongoDB.Driver;
using OnHive.Core.Library.Entities.Events;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Events.Domain.Abstractions.Repositories;

namespace OnHive.Events.Repositories
{
    public class MauticIntegrationRepository : MongoRepositoryBase<MauticIntegration>, IMauticIntegrationRepository
    {
        public MauticIntegrationRepository(MongoDBSettings settings) : base(settings, "MauticIntegrations")
        {
        }

        public async Task<MauticIntegration> GetMauticIntegrationByTenantId(string tenantId, bool activeOnly)
        {
            var filter = Builders<MauticIntegration>.Filter.Eq(i => i.TenantId, tenantId);
            if (activeOnly)
            {
                filter = Builders<MauticIntegration>.Filter.And(filter, Builders<MauticIntegration>.Filter.Eq(i => i.IsActive, true));
            }
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<MauticIntegration>(Builders<MauticIntegration>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}