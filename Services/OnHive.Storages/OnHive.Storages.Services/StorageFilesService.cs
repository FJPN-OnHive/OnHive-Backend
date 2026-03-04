using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Storages;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Validations.Common;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Storages.Domain.Abstractions.Repositories;
using OnHive.Storages.Domain.Abstractions.Services;
using OnHive.Storages.Domain.Models;
using Serilog;
using System.Data;

namespace OnHive.Storages.Services
{
    public class StorageFilesService : IStorageFilesService
    {
        private readonly IStorageFilesRepository storagesRepository;
        private readonly StoragesApiSettings storagesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEventRegister eventRegister;

        public StorageFilesService(IStorageFilesRepository storagesRepository, StoragesApiSettings storagesApiSettings, IMapper mapper, IEventRegister eventRegister)
        {
            this.storagesRepository = storagesRepository;
            this.storagesApiSettings = storagesApiSettings;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<StorageFileDto> UploadFileAsync(Stream fileStream, StorageFileDto storageDto, LoggedUserDto loggedUser)
        {
            try
            {
                ValidateType(storageDto);

                storageDto.TenantId = loggedUser.User.TenantId;
                await ValidateDuplicate(storageDto);
                fileStream.Seek(0, SeekOrigin.Begin);

                var fileStorage = mapper.Map<StorageFile>(storageDto);
                if (storagesApiSettings.BaseUrl.EndsWith("/"))
                {
                    storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
                }
                fileStorage.TargetFolder = "files";
                await UploadFileToS3Async(fileStorage, fileStream);

                fileStorage.FileUrl = $"{storagesApiSettings.BaseUrl}/File/{fileStorage.TenantId}/{fileStorage.FileId}";
                await storagesRepository.SaveAsync(fileStorage, loggedUser.User.Id);
                return mapper.Map<StorageFileDto>(fileStorage);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<StorageFileDto> UploadFileAsync(Stream fileStream, StorageFileDto storageDto, string targetFolder = "files")
        {
            try
            {
                ValidateType(storageDto);

                storageDto.TenantId = !string.IsNullOrEmpty(storageDto.TenantId) ? storageDto.TenantId : throw new ArgumentException("TenantId is required");
                await ValidateDuplicate(storageDto);
                fileStream.Seek(0, SeekOrigin.Begin);

                var fileStorage = mapper.Map<StorageFile>(storageDto);
                if (storagesApiSettings.BaseUrl.EndsWith("/"))
                {
                    storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
                }
                fileStorage.TargetFolder = targetFolder;
                await UploadFileToS3Async(fileStorage, fileStream);

                if (storageDto.Public)
                {
                    fileStorage.FileUrl = $"{storagesApiSettings.BaseUrl}/File/{fileStorage.TenantId}/{fileStorage.FileId}";
                }
                else
                {
                    fileStorage.FileUrl = $"{storagesApiSettings.BaseUrl}/PrivateFile/{fileStorage.FileId}";
                }
                await storagesRepository.SaveAsync(fileStorage);
                return mapper.Map<StorageFileDto>(fileStorage);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string fileId, LoggedUserDto loggedUser)
        {
            var storageFile = await storagesRepository.GetByFileIdAsync(fileId, loggedUser.User.TenantId, false);
            if (storageFile == null)
            {
                throw new NotFoundException("File not found");
            }
            return await DownloadFileAsync(storageFile, new MemoryStream());
        }

        public async Task<Stream> GetFileAsync(string fileId, string tenantId)
        {
            var storageFile = await storagesRepository.GetByFileIdAsync(fileId, tenantId, true);
            if (storageFile == null)
            {
                throw new NotFoundException("File not found");
            }
            return await DownloadFileAsync(storageFile, new MemoryStream());
        }

        public async Task<StorageFileDto?> GetByIdAsync(string storageId)
        {
            var storage = await storagesRepository.GetByIdAsync(storageId);
            return mapper.Map<StorageFileDto>(storage);
        }

        public async Task<PaginatedResult<StorageFileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await storagesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<StorageFileDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<StorageFileDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StorageFileDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<StorageFileDto>()
            };
        }

        public async Task<IEnumerable<StorageFileDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var storages = await storagesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<StorageFileDto>>(storages);
        }

        public async Task<StorageFileDto> SaveAsync(StorageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            var storage = mapper.Map<StorageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            storage.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            storage.CreatedAt = DateTime.UtcNow;
            storage.CreatedBy = string.IsNullOrEmpty(storage.CreatedBy) ? loggedUser?.User?.Id : storage.CreatedBy;
            var response = await storagesRepository.SaveAsync(storage);
            return mapper.Map<StorageFileDto>(response);
        }

        public async Task<StorageFileDto> CreateAsync(StorageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            if (!storageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var storage = mapper.Map<StorageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            storage.Id = string.Empty;
            storage.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await storagesRepository.SaveAsync(storage, loggedUser.User.Id);
            return mapper.Map<StorageFileDto>(response);
        }

        public async Task<StorageFileDto?> UpdateAsync(StorageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            if (!storageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var storage = mapper.Map<StorageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            var currentStorage = await storagesRepository.GetByIdAsync(storage.Id);
            if (currentStorage == null || currentStorage.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await storagesRepository.SaveAsync(storage, loggedUser.User.Id);
            return mapper.Map<StorageFileDto>(response);
        }

        public async Task<bool> DeleteFileAsync(string id, LoggedUserDto loggedUser)
        {
            var storageFile = await storagesRepository.GetByIdAsync(id);
            if (storageFile == null)
            {
                throw new NotFoundException("File not found");
            }
            ValidatePermissions(storageFile, loggedUser.User);
            await RemoveFileAsync(storageFile);
            await storagesRepository.DeleteAsync(id);
            return true;
        }

        private async Task<string> UploadFileToS3Async(StorageFile storageFile, Stream file)
        {
            var fileName = $"{storageFile.FileId}";

            try
            {
                var credentials = new BasicAWSCredentials(storagesApiSettings.BucketKeyId, storagesApiSettings.BucketSecret);
                var config = new AmazonS3Config
                {
                    ServiceURL = storagesApiSettings.BucketRegion,
                    ForcePathStyle = true,
                    UseHttp = false,
                    Timeout = TimeSpan.FromMinutes(10),
                };
                using var s3Client = new AmazonS3Client(credentials, config);
                var transferUtility = new TransferUtility(s3Client);
                await transferUtility.UploadAsync(file, storagesApiSettings.BucketName, $"{storageFile.TenantId}/{storageFile.TargetFolder}/{fileName}");
                Register(storageFile, EventKeys.FileUploaded, "File uploaded file to S3");
                return fileName;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error uploading file to S3: {fileName}");
                Register(storageFile, EventKeys.FileError, "Error uploading file to S3");
                throw new FileLoadException($"Error uploading file to S3: {fileName}", ex);
                throw;
            }
        }

        private async Task<Stream> DownloadFileAsync(StorageFile storageFile, MemoryStream file)
        {
            if (string.IsNullOrEmpty(storageFile.TargetFolder))
            {
                storageFile.TargetFolder = "files";
            }

            var credentials = new BasicAWSCredentials(storagesApiSettings.BucketKeyId, storagesApiSettings.BucketSecret);
            var config = new AmazonS3Config
            {
                ServiceURL = storagesApiSettings.BucketRegion,
                ForcePathStyle = true,
                UseHttp = false,
                Timeout = TimeSpan.FromMinutes(10),
            };
            using var s3Client = new AmazonS3Client(credentials, config);
            var transferUtility = new TransferUtility(s3Client);
            var fileName = $"{storageFile.FileId}";
            try
            {
                return await transferUtility.OpenStreamAsync(storagesApiSettings.BucketName, $"{storageFile.TenantId}/{storageFile.TargetFolder}/{fileName}");
            }
            catch (Exception)
            {
                throw new NotFoundException("File not found");
            }
        }

        private async Task RemoveFileAsync(StorageFile storageFile)
        {
            if (string.IsNullOrEmpty(storageFile.TargetFolder))
            {
                storageFile.TargetFolder = "files";
            }
            var credentials = new BasicAWSCredentials(storagesApiSettings.BucketKeyId, storagesApiSettings.BucketSecret);
            var config = new AmazonS3Config
            {
                ServiceURL = storagesApiSettings.BucketRegion,
                ForcePathStyle = true,
                UseHttp = false,
                Timeout = TimeSpan.FromMinutes(10),
            };
            using var s3Client = new AmazonS3Client(credentials, config);
            var fileName = $"{storageFile.FileId}";
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = storagesApiSettings.BucketName,
                    Key = $"{storageFile.TenantId}/{storageFile.TargetFolder}/{fileName}"
                };
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (Exception)
            {
                throw new NotFoundException("File not found");
            }
        }

        private void ValidateType(StorageFileDto storageDto)
        {
            var extenssion = Path.GetExtension(storageDto.FileId);
            if (!storagesApiSettings.ValidFileTypes.Contains(extenssion))
            {
                throw new ArgumentException("Invalid file type");
            }
        }

        private async Task ValidateDuplicate(StorageFileDto storageDto)
        {
            var storage = await storagesRepository.GetByFileIdAsync(storageDto.FileId, storageDto.TenantId, false);
            if (storage != null)
            {
                throw new DuplicateNameException("File already exists");
            }
        }

        private void ValidatePermissions(StorageFile storage, UserDto? loggedUser)
        {
            if (loggedUser != null && storage.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Storage/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    storage.Id, storage.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private void Register(StorageFile file, string key, string message)
        {
            _ = eventRegister.RegisterEvent(file.TenantId, file.CreatedBy, key, message, new Dictionary<string, string>
            {
                { "Id", file.Id },
                { "FileName", file.OriginalFileName },
                { "FileId", file.FileId },
                { "FileUrl", file.FileUrl }
            });
        }
    }
}