using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Storages;

namespace EHive.Storages.Domain.Abstractions.Repositories
{
    public interface IStorageImagesRepository : IRepositoryBase<StorageImageFile>
    {
        Task<PaginatedResult<StorageImageFile>> GetByFilterPublicAsync(RequestFilter filter, string tenantId);

        Task<StorageImageFile> GetByImageIdAsync(string imageId, string tenantId, bool publicOnly);
    }
}