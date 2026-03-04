using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Database.Library.Models;
using LiteDB;
using MongoDB.Driver;

namespace OnHive.Database.Library.LiteDb
{
    public abstract class LiteDBRepositoryBase<Entity> : IRepositoryBase<Entity> where Entity : EntityBase
    {
        protected readonly string collectionName;
        protected readonly LiteDatabase dataBase;
        protected readonly ILiteCollection<Entity> collection;

        protected LiteDBRepositoryBase(LiteDBSettings settings, string collectionName)
        {
            this.collectionName = collectionName;
            dataBase = new LiteDatabase(@$"Filename={settings.DataBasePath}; Connection=Shared;");
            collection = dataBase.GetCollection<Entity>(collectionName);
            collection.EnsureIndex(e => e.TenantId);
            CreateIndexes();
            _ = UpdateVersionsAsync();
        }

        public virtual async Task<Entity?> GetByIdAsync(string id)
        {
            return await Task.Run(() =>
            {
                return collection.FindOne(e => e.Id == id);
            });
        }

        public async Task<List<Entity>> GetAllAsync(string? tenantId)
        {
            return await Task.Run(() =>
            {
                return collection.Query().Where(e => e.TenantId == tenantId).ToList();
            });
        }

        public virtual async Task<Entity?> SaveAsync(Entity entity, string byUserId = "")
        {
            return await Task.Run(() =>
             {
                 if (string.IsNullOrEmpty(entity.Id))
                 {
                     entity.Id = Guid.NewGuid().ToString();
                     entity.CreatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
                     entity.CreatedAt = DateTime.UtcNow;
                 }
                 entity.UpdatedBy = string.IsNullOrEmpty(byUserId) ? entity.Id : byUserId;
                 entity.UpdatedAt = DateTime.UtcNow;
                 collection.Upsert(entity);
                 return entity;
             });
        }

        public virtual async Task DeleteAsync(string id)
        {
            await Task.Run(() =>
            {
                return collection.Delete(id);
            });
        }

        public virtual async Task<List<Entity>> SaveManyAsync(List<Entity> entities)
        {
            return await Task.Run(() =>
            {
                foreach (var entity in entities)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = Guid.NewGuid().ToString();
                    }
                    collection.Upsert(entity);
                }
                return entities;
            });
        }

        public async Task<Entity?> SaveVersionAsync(Entity entity, string byUserId = "")
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
            collection.Upsert(entity);
            return entity;
        }

        public virtual async Task<long> DeleteManyAsync(List<string> ids)
        {
            return await Task.Run(() =>
            {
                var result = 0;
                foreach (var id in ids)
                {
                    if (collection.FindById(id) != null)
                    {
                        collection.Delete(id);
                        result++;
                    }
                }
                return result;
            });
        }

        public Task<PaginatedResult<Entity>> GetByFilterAsync(RequestFilter filter, string? tenantId, bool activeOnly = true)
        {
            throw new NotSupportedException();
        }

        public Task<PaginatedResult<Entity>> GetByFilterAndIdsAsync(RequestFilter filter, List<string> ids, string? tenantId, bool activeOnly = true)
        {
            throw new NotSupportedException();
        }

        protected abstract void CreateIndexes();

        public async Task<List<Entity>> GetByVIdAsync(string vId)
        {
            return await Task.Run(() =>
            {
                return collection.Find(e => e.VId == vId).ToList();
            });
        }

        public async Task<Entity?> GetByVIdAsync(string vId, int versionNumber)
        {
            return await Task.Run(() =>
            {
                return collection.FindOne(e => e.VId == vId && e.VersionNumber == versionNumber);
            });
        }

        public async Task<Entity?> GetByVIdLatestAsync(string vId, bool activeOnly = true)
        {
            return await Task.Run(() =>
            {
                var versions = collection.Find(e => e.VId == vId);
                if (activeOnly)
                {
                    versions = versions.Where(e => e.IsActive);
                }
                var versionNumber = versions.Max(e => e.VersionNumber);
                return collection.FindOne(e => e.VId == vId && e.VersionNumber == versionNumber);
            });
        }

        public async Task<Entity?> GetByVIdActiveAsync(string vId)
        {
            return await Task.Run(() =>
            {
                return collection.FindOne(e => e.VId == vId && e.ActiveVersion);
            });
        }

        public async Task<long> DeleteByVIdAsync(string vId)
        {
            return await Task.Run(() =>
            {
                var versions = collection.Find(e => e.VId == vId);
                var result = 0;
                foreach (var item in versions)
                {
                    if (collection.FindById(item.Id) != null)
                    {
                        collection.Delete(item.Id);
                        result++;
                    }
                }
                return result;
            });
        }

        public Task UpdateVersionsAsync()
        {
            return Task.Run(() =>
            {
                var result = collection.Find(e => string.IsNullOrEmpty(e.VId)).ToList();

                foreach (var item in result)
                {
                    item.VId = item.Id;
                    item.VersionNumber = 1;
                    collection.Update(item);
                }
            });
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
            foreach (var item in versions)
            {
                collection.Upsert(item);
            }
            return versions.Find(v => v.ActiveVersion);
        }
    }
}