using MongoDB.Driver;
using OnHive.Core.Library.Entities.Certificates;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Certificates.Domain.Abstractions.Repositories;

namespace OnHive.Certificates.Repositories
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
