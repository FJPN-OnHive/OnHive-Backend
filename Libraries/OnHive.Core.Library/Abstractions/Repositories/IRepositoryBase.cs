using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities;

namespace EHive.Core.Library.Abstractions.Repositories
{
    public interface IRepositoryBase<Entity> where Entity : EntityBase
    {
        Task DeleteAsync(string id);

        Task<long> DeleteManyAsync(List<string> ids);

        Task<Entity?> GetByIdAsync(string id);

        Task<List<Entity>> GetAllAsync(string? tenantId);

        Task<PaginatedResult<Entity>> GetByFilterAsync(RequestFilter filter, string? tenantId, bool activeOnly = true);

        Task<PaginatedResult<Entity>> GetByFilterAndIdsAsync(RequestFilter filter, List<string> ids, string? tenantId, bool activeOnly = true);

        Task<Entity?> SaveAsync(Entity entity, string byUserId = "");

        Task<List<Entity>> SaveManyAsync(List<Entity> entities);

        Task<Entity?> SaveVersionAsync(Entity entity, string byUserId = "");

        Task<List<Entity>> GetByVIdAsync(string vId);

        Task<Entity?> GetByVIdAsync(string vId, int versionNumber);

        Task<Entity?> SetActiveVersionAsync(string vId, int versionNumber);

        Task<Entity?> GetByVIdLatestAsync(string vId, bool activeOnly = true);

        Task<Entity?> GetByVIdActiveAsync(string vId);

        Task<long> DeleteByVIdAsync(string vId);

        Task UpdateVersionsAsync();
    }
}