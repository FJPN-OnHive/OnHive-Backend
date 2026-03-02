using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Storages;
using EHive.Core.Library.Entities.Storages;
using EHive.Core.Library.Entities.Storages;

namespace EHive.Storages.Domain.Abstractions.Repositories
{
    public interface IStorageFilesRepository : IRepositoryBase<StorageFile>
    {
        Task<StorageFile> GetByFileIdAsync(string fileId, string tenantId, bool publicOnly);
    }
}