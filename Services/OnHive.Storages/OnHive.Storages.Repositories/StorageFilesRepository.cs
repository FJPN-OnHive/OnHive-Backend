using MongoDB.Driver;
using OnHive.Core.Library.Entities.Storages;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Storages.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Storages;

namespace OnHive.Storages.Repositories
{
    public class StorageFilesRepository : MongoRepositoryBase<StorageFile>, IStorageFilesRepository
    {
        public StorageFilesRepository(MongoDBSettings settings) : base(settings, "StorageFiles")
        {
        }

        public async Task<StorageFile> GetByFileIdAsync(string fileId, string tenantId, bool publicOnly)
        {
            var filter = Builders<StorageFile>.Filter.Eq(i => i.FileId, fileId)
                & Builders<StorageFile>.Filter.Eq(i => i.TenantId, tenantId);
            if (publicOnly)
            {
                filter &= Builders<StorageFile>.Filter.Eq(i => i.Public, true);
            }
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<StorageFile>(Builders<StorageFile>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StorageFile>(Builders<StorageFile>.IndexKeys.Ascending(i => i.FileId)));
        }
    }
}