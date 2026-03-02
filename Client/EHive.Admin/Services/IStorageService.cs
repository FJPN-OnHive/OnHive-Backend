using EHive.Core.Library.Contracts.Storages;

namespace EHive.Admin.Services
{
    public interface IStorageService : IServiceBase<StorageFileDto>
    {
        Task<StorageImageFileDto> UploadImageAsync(StorageImageFileDto file, Stream image, bool noConvert, string token);

        Task<StorageImageFileDto> UpdateImageAsync(StorageImageFileDto file, string token);

        Task<bool> DeleteImageAsync(string id, string token);

        Task<List<StorageImageFileDto>> GetImagesAsync(string token);

        Task<List<StorageFileDto>> GetFilesAsync(string token);

        Task<StorageFileDto> UploadFileAsync(StorageFileDto file, Stream fileStream, string token);

        Task<StorageFileDto> UpdateFileAsync(StorageFileDto file, string token);

        Task<bool> DeleteFileAsync(string id, string token);

        Task<Stream> GetPrivateFileStreamAsync(string fileId, string token);

        Task<Stream> GetPublicFileStreamAsync(string fileId, string tenantId);
    }
}