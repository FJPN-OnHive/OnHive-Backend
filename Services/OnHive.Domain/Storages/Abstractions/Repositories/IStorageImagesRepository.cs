using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Storages;

namespace OnHive.Storages.Domain.Abstractions.Repositories
{
    public interface IStorageImagesRepository : IRepositoryBase<StorageImageFile>
    {
        Task<PaginatedResult<StorageImageFile>> GetByFilterPublicAsync(RequestFilter filter, string tenantId);

        Task<StorageImageFile> GetByImageIdAsync(string imageId, string tenantId, bool publicOnly);
    }
}