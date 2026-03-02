using MongoDB.Driver;
using EHive.Core.Library.Entities.Storages;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Storages.Domain.Abstractions.Repositories;
using EHive.Core.Library.Entities.Storages;

namespace EHive.Storages.Repositories
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