using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Storages;

namespace OnHive.Storages.Domain.Abstractions.Services
{
    public interface IStorageFilesService
    {
        Task<StorageFileDto> UploadFileAsync(Stream fileStream, StorageFileDto storageDto, LoggedUserDto loggedUser);

        Task<StorageFileDto> UploadFileAsync(Stream fileStream, StorageFileDto storageDto, string targetFolder = "files");

        Task<Stream> GetFileAsync(string fileId, LoggedUserDto loggedUser);

        Task<Stream> GetFileAsync(string fileId, string tenantId);

        Task<StorageFileDto?> GetByIdAsync(string storageId);

        Task<PaginatedResult<StorageFileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<StorageFileDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<StorageFileDto> SaveAsync(StorageFileDto storageDto, LoggedUserDto? user);

        Task<StorageFileDto> CreateAsync(StorageFileDto storageDto, LoggedUserDto? loggedUser);

        Task<StorageFileDto?> UpdateAsync(StorageFileDto storageDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteFileAsync(string id, LoggedUserDto loggedUser);
    }
}