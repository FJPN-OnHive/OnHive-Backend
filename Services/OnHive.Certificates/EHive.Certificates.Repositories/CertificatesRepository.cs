using MongoDB.Driver;
using EHive.Core.Library.Entities.Certificates;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Certificates.Domain.Abstractions.Repositories;

namespace EHive.Certificates.Repositories
{
    public class CertificatesRepository : MongoRepositoryBase<Certificate>, ICertificatesRepository
    {
        public CertificatesRepository(MongoDBSettings settings) : base(settings, "Certificates")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Certificate>(Builders<Certificate>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}
