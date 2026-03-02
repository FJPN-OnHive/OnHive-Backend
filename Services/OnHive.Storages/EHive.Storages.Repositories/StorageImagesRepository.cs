using MongoDB.Driver;
using EHive.Core.Library.Entities.Storages;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Storages.Domain.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Helpers;

namespace EHive.Storages.Repositories
{
    public class StorageImagesRepository : MongoRepositoryBase<StorageImageFile>, IStorageImagesRepository
    {
        public StorageImagesRepository(MongoDBSettings settings) : base(settings, "StorageImages")
        {
        }

        public async Task<PaginatedResult<StorageImageFile>> GetByFilterPublicAsync(RequestFilter filter, string tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<StorageImageFile>(filter, tenantId, false);
            queryFilter = queryFilter & Builders<StorageImageFile>.Filter.Eq(i => i.Public, true);

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<StorageImageFile>(filter));
            }
            else
            {
                result.Sort(Builders<StorageImageFile>.Sort.Descending("UpdatedAt"));
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

            return new PaginatedResult<StorageImageFile>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = await result.CountDocumentsAsync(),
                Itens = await result.ToListAsync()
            };
        }

        public async Task<StorageImageFile> GetByImageIdAsync(string imageId, string tenantId, bool publicOnly)
        {
            var filter = Builders<StorageImageFile>.Filter.Eq(i => i.ImageId, imageId)
                & Builders<StorageImageFile>.Filter.Eq(i => i.TenantId, tenantId);
            if (publicOnly)
            {
                filter &= Builders<StorageImageFile>.Filter.Eq(i => i.Public, true);
            }
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<StorageImageFile>(Builders<StorageImageFile>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StorageImageFile>(Builders<StorageImageFile>.IndexKeys.Ascending(i => i.ImageId)));
        }
    }
}