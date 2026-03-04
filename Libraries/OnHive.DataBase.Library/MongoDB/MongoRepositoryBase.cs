using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities;
using OnHive.Core.Library.Helpers;
using OnHive.Database.Library.Models;
using MongoDB.Driver;

namespace OnHive.Database.Library.MongoDB
{
    public abstract class MongoRepositoryBase<Entity> : IRepositoryBase<Entity> where Entity : EntityBase
    {
        protected readonly string collectionName;
        protected readonly MongoClient mongoClient;
        protected readonly IMongoCollection<Entity> collection;

        protected MongoRepositoryBase(MongoDBSettings settings, string collectionName)
        {
            this.collectionName = collectionName;
            mongoClient = new MongoClient(settings.ConnectionString);
            collection = mongoClient.GetDatabase(settings.DataBase).GetCollection<Entity>(collectionName);
            collection.Indexes.CreateOne(new CreateIndexModel<Entity>(Builders<Entity>.IndexKeys.Ascending(i => i.TenantId)));
            UpdateVersionsAsync().Wait();
            var versionIndexDefinition = Builders<Entity>.IndexKeys
                .Ascending(i => i.TenantId)
                .Ascending(i => i.VId)
                .Ascending(i => i.VersionNumber);
            collection.Indexes.CreateOne(
                new CreateIndexModel<Entity>(versionIndexDefinition, new CreateIndexOptions { Unique = true }));
            CreateIndexes();
        }

        public virtual async Task<PaginatedResult<Entity>> GetByFilterAsync(RequestFilter filter, string? tenantId, bool activeOnly = true)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<Entity>(filter, tenantId, activeOnly);

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result = result.Sort(MongoDbFilterConverter.ConvertSort<Entity>(filter));
            }
            else
            {
                result = result.Sort(Builders<Entity>.Sort.Descending("UpdatedAt"));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result = result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<Entity>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = await result.CountDocumentsAsync(),
                Itens = await result.ToListAsync()
            };
        }

        public async Task<PaginatedResult<Entity>> GetByFilterAndIdsAsync(RequestFilter filter, List<string> ids, string? tenantId, bool activeOnly = true)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<Entity>(filter, tenantId, activeOnly);

            if (ids.Any())
            {
                queryFilter = Builders<Entity>.Filter.And(queryFilter, Builders<Entity>.Filter.In(e => e.Id, ids));
            }

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result = result.Sort(MongoDbFilterConverter.ConvertSort<Entity>(filter));
            }
            else
            {
                result = result.Sort(Builders<Entity>.Sort.Descending("UpdatedAt"));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result = result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<Entity>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = await result.CountDocumentsAsync(),
                Itens = await result.ToListAsync()
            };
        }

        public virtual async Task<Entity?> GetByIdAsync(string id)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.Id, id);
            var result = await collection.FindAsync(filter);
            return result.FirstOrDefault();
        }

        public virtual async Task<List<Entity>> GetByVIdAsync(string vId)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.VId, vId);
            var result = await collection.FindAsync(filter);
            return result.ToList();
        }

        public virtual async Task<Entity?> GetByVIdAsync(string vId, int versionNumber)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.VId, vId)
                & Builders<Entity>.Filter.Eq(e => e.VersionNumber, versionNumber);
            var result = await collection.FindAsync(filter);
            return result.FirstOrDefault();
        }

        public virtual async Task<Entity?> GetByVIdLatestAsync(string vId, bool activeOnly = true)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.VId, vId);

            if (activeOnly)
            {
                filter = filter & Builders<Entity>.Filter.Eq(e => e.IsActive, true);
            }

            var result = await collection.Find(filter).ToListAsync();
            return result.Find(e => e.VersionNumber == result.Max(r => r.VersionNumber));
        }

        public async Task<Entity?> GetByVIdActiveAsync(string vId)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.VId, vId)
                & Builders<Entity>.Filter.Eq(e => e.ActiveVersion, true);
            var result = await collection.FindAsync(filter);
            return result.FirstOrDefault();
        }

        public async Task<List<Entity>> GetAllAsync(string? tenantId)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.TenantId, tenantId);
            return await collection.Find(filter).ToListAsync();
        }

        public virtual async Task<Entity?> SaveAsync(Entity entity, string byUserId = "")
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.IsActive = true;
                entity.CreatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
                entity.CreatedAt = DateTime.UtcNow;
            }
            if (string.IsNullOrEmpty(entity.VId))
            {
                entity.VId = entity.Id;
                entity.VersionNumber = 1;
            }
            entity.UpdatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<Entity>.Filter.Eq(e => e.Id, entity.Id);
            await collection.FindOneAndReplaceAsync(filter,
                                                    entity,
                                                    new FindOneAndReplaceOptions<Entity, Entity>() { IsUpsert = true });
            return entity;
        }

        public virtual async Task<Entity?> SaveVersionAsync(Entity entity, string byUserId = "")
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.IsActive = true;
            entity.CreatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
            entity.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(entity.VId))
            {
                entity.VId = entity.Id;
            }
            var lastVersion = await GetByVIdLatestAsync(entity.VId);
            if (lastVersion != null)
            {
                entity.VersionNumber = lastVersion.VersionNumber + 1;
            }
            else
            {
                entity.VersionNumber = 1;
            }
            entity.UpdatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<Entity>.Filter.Eq(e => e.Id, entity.Id);
            await collection.FindOneAndReplaceAsync(filter,
                                                    entity,
                                                    new FindOneAndReplaceOptions<Entity, Entity>() { IsUpsert = true });
            return await SetActiveVersionAsync(entity.VId, entity.VersionNumber);
        }

        public virtual async Task DeleteAsync(string id)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.Id, id);
            await collection.FindOneAndDeleteAsync(filter);
        }

        public virtual async Task<List<Entity>> SaveManyAsync(List<Entity> entities)
        {
            var bulkOps = new List<WriteModel<Entity>>();
            foreach (var record in entities)
            {
                if (string.IsNullOrEmpty(record.Id))
                {
                    record.Id = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrEmpty(record.VId))
                {
                    record.VId = record.Id;
                    record.VersionNumber = 1;
                }
                var upsertOne = new ReplaceOneModel<Entity>(
                    Builders<Entity>.Filter.Where(x => x.Id == record.Id),
                    record)
                { IsUpsert = true };
                bulkOps.Add(upsertOne);
            }
            await collection.BulkWriteAsync(bulkOps);
            return entities;
        }

        public virtual async Task<long> DeleteManyAsync(List<string> ids)
        {
            var result = await collection.DeleteManyAsync(e => ids.Contains(e.Id ?? ""));
            return result.DeletedCount;
        }

        public virtual async Task<long> DeleteByVIdAsync(string vId)
        {
            var filter = Builders<Entity>.Filter.Eq(e => e.VId, vId);
            var result = await collection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task UpdateVersionsAsync()
        {
            var filter = Builders<Entity>.Filter.Not(Builders<Entity>.Filter.Exists(e => e.VId))
                | Builders<Entity>.Filter.Eq(e => e.VId, string.Empty);

            var result = await collection.Find(filter).ToListAsync();

            foreach (var item in result)
            {
                item.VId = item.Id;
                item.VersionNumber = 1;
                var updateFilter = Builders<Entity>.Filter.Eq(e => e.Id, item.Id);
                await collection.FindOneAndReplaceAsync(updateFilter,
                                                        item,
                                                        new FindOneAndReplaceOptions<Entity, Entity>() { IsUpsert = true });
            }
        }

        public async Task<Entity?> SetActiveVersionAsync(string vId, int versionNumber)
        {
            var versions = await GetByVIdAsync(vId);
            foreach (var item in versions)
            {
                item.ActiveVersion = item.VersionNumber == versionNumber;
            }
            if (!versions.Exists(v => v.ActiveVersion))
            {
                versions.Last().ActiveVersion = true;
            }
            await collection.BulkWriteAsync(versions.Select(item =>
                new ReplaceOneModel<Entity>(
                    Builders<Entity>.Filter.Where(x => x.Id == item.Id),
                    item)
                { IsUpsert = true }
            ).ToList());
            return versions.Find(v => v.ActiveVersion);
        }

        protected abstract void CreateIndexes();
    }
}