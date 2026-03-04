using MongoDB.Driver;
using OnHive.Core.Library.Entities.Certificates;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Certificates.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Helpers;

namespace OnHive.Certificates.Repositories
{
    public class CertificateMountsRepository : MongoRepositoryBase<CertificateMount>, ICertificateMountsRepository
    {
        public CertificateMountsRepository(MongoDBSettings settings) : base(settings, "CertificateMounts")
        {
        }

        public async Task<PaginatedResult<CertificateMount>> GetByFilterUserAsync(RequestFilter filter, string userId, string? tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<CertificateMount>(filter, tenantId, true);
            queryFilter = queryFilter & Builders<CertificateMount>.Filter.Eq(i => i.UserId, userId);

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<CertificateMount>(filter));
            }
            else
            {
                result.Sort(Builders<CertificateMount>.Sort.Descending("UpdatedAt"));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<CertificateMount>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = await result.CountDocumentsAsync(),
                Itens = await result.ToListAsync()
            };
        }

        public Task<CertificateMount> GetByKeyAsync(string certificateKey)
        {
            var filter = Builders<CertificateMount>.Filter.Eq(i => i.CertificateKey, certificateKey);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<CertificateMount>(Builders<CertificateMount>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<CertificateMount>(Builders<CertificateMount>.IndexKeys.Ascending(i => i.CertificateKey)));
        }

        public async Task<string> CertificateExistsAsync(string studentId, string courseId, string tenantId)
        {
            var filter = Builders<CertificateMount>.Filter.And(
                Builders<CertificateMount>.Filter.Eq("StudentId", studentId),
                Builders<CertificateMount>.Filter.Eq("CourseId", courseId),
                Builders<CertificateMount>.Filter.Eq("TenantId", tenantId),
                Builders<CertificateMount>.Filter.Eq("IsActive", true)
            );

            var result = await collection.Find(filter).FirstOrDefaultAsync();
            return result != null ? result.Id : string.Empty;
        }
    }
}