using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Storages;

namespace EHive.Storages.Domain.Abstractions.Services
{
    public interface IStorageImagesService
    {
        Task<StorageImageFileDto> UploadImageAsync(Stream imageStream, StorageImageFileDto storageDto, bool noConvert, LoggedUserDto loggedUser);

        Task<Stream> GetImageAsync(string imageId, string resolution, LoggedUserDto loggedUser);

        Task<Stream> GetImageAsync(string imageId, string resolution, string tenantId);

        Task<StorageImageFileDto?> GetByIdAsync(string storageId);

        Task<PaginatedResult<StorageImageFileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<StorageImageFileDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<StorageImageFileDto> SaveAsync(StorageImageFileDto storageDto, LoggedUserDto? user);

        Task<StorageImageFileDto> CreateAsync(StorageImageFileDto storageDto, LoggedUserDto? loggedUser);

        Task<StorageImageFileDto?> UpdateAsync(StorageImageFileDto storageDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteImageAsync(string id, LoggedUserDto loggedUser);

        Task<PaginatedResult<StorageImageFileDto>> GetByFilterOpenAsync(RequestFilter filter, string tenantId);
    }
}