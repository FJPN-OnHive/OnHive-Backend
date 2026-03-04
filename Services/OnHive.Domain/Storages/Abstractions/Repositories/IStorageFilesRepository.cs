using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Entities.Storages;
using OnHive.Core.Library.Entities.Storages;

namespace OnHive.Storages.Domain.Abstractions.Repositories
{
    public interface IStorageFilesRepository : IRepositoryBase<StorageFile>
    {
        Task<StorageFile> GetByFileIdAsync(string fileId, string tenantId, bool publicOnly);
    }
}