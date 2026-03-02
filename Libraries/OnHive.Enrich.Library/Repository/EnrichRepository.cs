using EHive.Core.Library.Entities.Enrichments;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Enrich.Library.Repository
{
    public class EnrichRepository : MongoRepositoryBase<Enrichment>
    {
        public EnrichRepository(MongoDBSettings settings) : base(settings, "Enrichments")
        {
        }

        public Task<Enrichment> GetEnrich(string tenantId, string entityId, string entityType)
        {
            var filter = Builders<Enrichment>.Filter.Eq(e => e.TenantId, tenantId) &
                         Builders<Enrichment>.Filter.Eq(e => e.EntityId, entityId) &
                         Builders<Enrichment>.Filter.Eq(e => e.EntityType, entityType);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            var indexDefinition = Builders<Enrichment>.IndexKeys
              .Ascending(e => e.TenantId)
              .Ascending(e => e.EntityId)
              .Ascending(e => e.EntityType);
            collection.Indexes.CreateOne(
                new CreateIndexModel<Enrichment>(indexDefinition, new CreateIndexOptions { Unique = true }));
        }
    }
}