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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Data;

namespace OnHive.Storages.Services
{
    public class StorageImagesService : IStorageImagesService
    {
        private readonly IStorageImagesRepository storagesRepository;
        private readonly StoragesApiSettings storagesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEventRegister eventRegister;

        public StorageImagesService(IStorageImagesRepository storagesRepository, StoragesApiSettings storagesApiSettings, IMapper mapper, IEventRegister eventRegister)
        {
            this.storagesRepository = storagesRepository;
            this.storagesApiSettings = storagesApiSettings;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<StorageImageFileDto> UploadImageAsync(Stream imageStream, StorageImageFileDto storageDto, bool noConvert, LoggedUserDto loggedUser)
        {
            StorageImageFileDto result;
            if (storageDto.OriginalFileName.EndsWith(".svg"))
            {
                result = await SvgPipeline(imageStream, storageDto, loggedUser);
            }
            else if (storageDto.OriginalFileName.EndsWith(".gif"))
            {
                result = await GifPipeline(imageStream, storageDto, loggedUser);
            }
            else
            {
                if (!noConvert)
                {
                    result = await WeppPipeline(imageStream, storageDto, loggedUser);
                }
                else
                {
                    result = await DefaultPipeline(imageStream, storageDto, loggedUser);
                }
            }
            imageStream.Dispose();
            return result;
        }

        public async Task<Stream> GetImageAsync(string imageId, string resolution, LoggedUserDto loggedUser)
        {
            var storageFile = await storagesRepository.GetByImageIdAsync(imageId, loggedUser.User.TenantId, false);
            if (storageFile == null)
            {
                throw new NotFoundException("Image not found");
            }
            if (storageFile.ImageId.EndsWith(".svg") || storageFile.ImageId.EndsWith(".gif"))
            {
                resolution = "hires";
            }
            return await DownloadImageAsync(storageFile, resolution);
        }

        public async Task<Stream> GetImageAsync(string imageId, string resolution, string tenantId)
        {
            var storageFile = await storagesRepository.GetByImageIdAsync(imageId, tenantId, true);
            if (storageFile == null)
            {
                throw new NotFoundException("Image not found");
            }
            if (storageFile.ImageId.EndsWith(".svg") || storageFile.ImageId.EndsWith(".gif"))
            {
                resolution = "hires";
            }
            return await DownloadImageAsync(storageFile, resolution);
        }

        public async Task<bool> DeleteImageAsync(string id, LoggedUserDto loggedUser)
        {
            var storageFile = await storagesRepository.GetByIdAsync(id);
            if (storageFile == null)
            {
                throw new NotFoundException("Image not found");
            }
            ValidatePermissions(storageFile, loggedUser.User);
            if (storageFile.ImageId.EndsWith(".svg") || storageFile.ImageId.EndsWith(".gif"))
            {
                await RemoveImageAsync(storageFile, "hires");
                await storagesRepository.DeleteAsync(id);
                return true;
            }
            await RemoveImageAsync(storageFile, "lowres");
            await RemoveImageAsync(storageFile, "midres");
            await RemoveImageAsync(storageFile, "hires");
            await storagesRepository.DeleteAsync(id);
            return true;
        }

        public async Task<StorageImageFileDto?> GetByIdAsync(string storageId)
        {
            var storage = await storagesRepository.GetByIdAsync(storageId);
            return mapper.Map<StorageImageFileDto>(storage);
        }

        public async Task<PaginatedResult<StorageImageFileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await storagesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<StorageImageFileDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<StorageImageFileDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StorageImageFileDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<StorageImageFileDto>()
            };
        }

        public async Task<PaginatedResult<StorageImageFileDto>> GetByFilterOpenAsync(RequestFilter filter, string tenantId)
        {
            var result = await storagesRepository.GetByFilterPublicAsync(filter, tenantId);
            if (result != null)
            {
                return new PaginatedResult<StorageImageFileDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<StorageImageFileDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StorageImageFileDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<StorageImageFileDto>()
            };
        }

        public async Task<IEnumerable<StorageImageFileDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var storages = await storagesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<StorageImageFileDto>>(storages);
        }

        public async Task<StorageImageFileDto> SaveAsync(StorageImageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            var storage = mapper.Map<StorageImageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            storage.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            storage.CreatedAt = DateTime.UtcNow;
            storage.CreatedBy = string.IsNullOrEmpty(storage.CreatedBy) ? loggedUser?.User?.Id : storage.CreatedBy;

            var response = await storagesRepository.SaveAsync(storage);
            return mapper.Map<StorageImageFileDto>(response);
        }

        public async Task<StorageImageFileDto> CreateAsync(StorageImageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            if (!storageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var storage = mapper.Map<StorageImageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            storage.Id = string.Empty;
            storage.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await storagesRepository.SaveAsync(storage, loggedUser.User.Id);
            return mapper.Map<StorageImageFileDto>(response);
        }

        public async Task<StorageImageFileDto?> UpdateAsync(StorageImageFileDto storageDto, LoggedUserDto? loggedUser)
        {
            if (!storageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var storage = mapper.Map<StorageImageFile>(storageDto);
            ValidatePermissions(storage, loggedUser?.User);
            var currentStorage = await storagesRepository.GetByIdAsync(storage.Id);
            if (currentStorage == null || currentStorage.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await storagesRepository.SaveAsync(storage, loggedUser.User.Id);
            return mapper.Map<StorageImageFileDto>(response);
        }

        public async Task<Stream> DownloadImageAsync(StorageImageFile storageFile, string prefix)
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
            var fileName = $"{prefix}_{storageFile.ImageId}";
            try
            {
                return await transferUtility.OpenStreamAsync(storagesApiSettings.BucketName, $"{storageFile.TenantId}/images/{fileName}");
            }
            catch (Exception)
            {
                throw new NotFoundException("Image not found");
            }
        }

        public async Task RemoveImageAsync(StorageImageFile storageFile, string prefix)
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
            var fileName = $"{prefix}_{storageFile.ImageId}";
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = storagesApiSettings.BucketName,
                    Key = $"{storageFile.TenantId}/images/{fileName}"
                };
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (Exception)
            {
                throw new NotFoundException("Image not found");
            }
        }

        private async Task<StorageImageFileDto> SvgPipeline(Stream imageStream, StorageImageFileDto storageDto, LoggedUserDto loggedUser)
        {
            if (storageDto.ImageId.Contains('.'))
            {
                storageDto.ImageId = storageDto.ImageId.Split('.')[0];
            }
            if (!storageDto.ImageId.EndsWith(".svg"))
            {
                storageDto.ImageId = $"{storageDto.ImageId}.svg";
            }
            storageDto.TenantId = loggedUser.User.TenantId;
            await ValidateDuplicate(storageDto);
            imageStream.Seek(0, SeekOrigin.Begin);

            var fileStorage = mapper.Map<StorageImageFile>(storageDto);
            if (storagesApiSettings.BaseUrl.EndsWith("/"))
            {
                storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
            }

            var hiResStream = new MemoryStream();
            imageStream.CopyTo(hiResStream);
            await UploadImageToS3Async(fileStorage, "hires", hiResStream);

            fileStorage.LowResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}"; ;
            fileStorage.MidResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}"; ;
            fileStorage.HiResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            await storagesRepository.SaveAsync(fileStorage, loggedUser.User.Id);
            hiResStream.Dispose();
            return mapper.Map<StorageImageFileDto>(fileStorage);
        }

        private async Task<StorageImageFileDto> GifPipeline(Stream imageStream, StorageImageFileDto storageDto, LoggedUserDto loggedUser)
        {
            if (storageDto.ImageId.Contains('.'))
            {
                storageDto.ImageId = storageDto.ImageId.Split('.')[0];
            }
            if (!storageDto.ImageId.EndsWith(".gif"))
            {
                storageDto.ImageId = $"{storageDto.ImageId}.gif";
            }
            storageDto.TenantId = loggedUser.User.TenantId;
            await ValidateDuplicate(storageDto);
            imageStream.Seek(0, SeekOrigin.Begin);

            var fileStorage = mapper.Map<StorageImageFile>(storageDto);
            if (storagesApiSettings.BaseUrl.EndsWith("/"))
            {
                storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
            }

            var hiResStream = new MemoryStream();
            imageStream.CopyTo(hiResStream);
            await UploadImageToS3Async(fileStorage, "hires", hiResStream);

            fileStorage.LowResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            fileStorage.MidResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            fileStorage.HiResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            await storagesRepository.SaveAsync(fileStorage, loggedUser.User.Id);
            hiResStream.Dispose();
            return mapper.Map<StorageImageFileDto>(fileStorage);
        }

        private async Task<StorageImageFileDto> DefaultPipeline(Stream imageStream, StorageImageFileDto storageDto, LoggedUserDto loggedUser)
        {
            if (!storageDto.ImageId.Contains('.'))
            {
                var extenssion = storageDto.OriginalFileName.Split(".")[1];
                storageDto.ImageId = $"{storageDto.ImageId}.{extenssion}";
            }
            storageDto.TenantId = loggedUser.User.TenantId;
            await ValidateDuplicate(storageDto);
            imageStream.Seek(0, SeekOrigin.Begin);

            var fileStorage = mapper.Map<StorageImageFile>(storageDto);
            if (storagesApiSettings.BaseUrl.EndsWith("/"))
            {
                storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
            }

            var hiResStream = new MemoryStream();
            imageStream.CopyTo(hiResStream);
            await UploadImageToS3Async(fileStorage, "hires", hiResStream);

            fileStorage.LowResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            fileStorage.MidResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            fileStorage.HiResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";
            await storagesRepository.SaveAsync(fileStorage, loggedUser.User.Id);
            hiResStream.Dispose();
            return mapper.Map<StorageImageFileDto>(fileStorage);
        }

        private async Task<StorageImageFileDto> WeppPipeline(Stream imageStream, StorageImageFileDto storageDto, LoggedUserDto loggedUser)
        {
            if (storageDto.ImageId.Contains('.'))
            {
                storageDto.ImageId = storageDto.ImageId.Split('.')[0];
            }
            if (!storageDto.ImageId.EndsWith(".webp"))
            {
                storageDto.ImageId = $"{storageDto.ImageId}.webp";
            }
            storageDto.TenantId = loggedUser.User.TenantId;
            await ValidateDuplicate(storageDto);
            imageStream.Seek(0, SeekOrigin.Begin);
            var format = Image.DetectFormat(imageStream);
            if (format == null)
            {
                throw new ArgumentException("Invalid image format");
            }
            Image image = Image.Load(imageStream);

            var lowRes = image.Clone(x => x.Resize(storagesApiSettings.LowResWidth, (storagesApiSettings.LowResWidth * image.Height) / image.Width));
            var midRes = image.Clone(x => x.Resize(storagesApiSettings.MidResWidth, (storagesApiSettings.MidResWidth * image.Height) / image.Width));

            var lowResStream = new MemoryStream();
            var midResStream = new MemoryStream();
            var hiResStream = new MemoryStream();

            if (storagesApiSettings.GenerateLowRes)
            {
                lowRes.SaveAsWebp(lowResStream, new WebpEncoder());
            }
            if (storagesApiSettings.GenerateMidRes)
            {
                midRes.SaveAsWebp(midResStream, new WebpEncoder());
            }
            if (image.Width > storagesApiSettings.HiResWidthLimit)
            {
                image.Mutate(x => x.Resize(storagesApiSettings.HiResWidthLimit, (storagesApiSettings.HiResWidthLimit * image.Height) / image.Width));
            }
            image.SaveAsWebp(hiResStream, new WebpEncoder());

            var fileStorage = mapper.Map<StorageImageFile>(storageDto);
            if (storagesApiSettings.BaseUrl.EndsWith("/"))
            {
                storagesApiSettings.BaseUrl = storagesApiSettings.BaseUrl.Substring(0, storagesApiSettings.BaseUrl.Length - 1);
            }

            if (storagesApiSettings.GenerateLowRes)
            {
                await UploadImageToS3Async(fileStorage, "lowres", lowResStream);
                fileStorage.LowResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}?res=lowres";
            }
            if (storagesApiSettings.GenerateMidRes)
            {
                await UploadImageToS3Async(fileStorage, "midres", midResStream);
                fileStorage.MidResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}?res=midres";
            }
            await UploadImageToS3Async(fileStorage, "hires", hiResStream);
            fileStorage.HiResImageUrl = $"{storagesApiSettings.BaseUrl}/Image/{fileStorage.TenantId}/{fileStorage.ImageId}";

            await storagesRepository.SaveAsync(fileStorage, loggedUser.User.Id);
            lowResStream.Dispose();
            midResStream.Dispose();
            hiResStream.Dispose();
            return mapper.Map<StorageImageFileDto>(fileStorage);
        }

        private async Task<string> UploadImageToS3Async(StorageImageFile storageFile, string prefix, MemoryStream file)
        {
            var fileName = $"{prefix}_{storageFile.ImageId}";
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
                await transferUtility.UploadAsync(file, storagesApiSettings.BucketName, $"{storageFile.TenantId}/images/{fileName}");
                Register(storageFile, EventKeys.ImageUploaded, "Image uploaded image to S3");
                return fileName;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error uploading image to S3: {fileName}");
                Register(storageFile, EventKeys.ImageError, "Error uploading image to S3");
                throw new FileLoadException($"Error uploading image to S3: {fileName}", ex);
            }
        }

        private async Task ValidateDuplicate(StorageImageFileDto storageDto)
        {
            var storage = await storagesRepository.GetByImageIdAsync(storageDto.ImageId, storageDto.TenantId, false);
            if (storage != null)
            {
                throw new DuplicateNameException("Image already exists");
            }
        }

        private void ValidatePermissions(StorageImageFile storage, UserDto? loggedUser)
        {
            if (loggedUser != null && storage.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Storage/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    storage.Id, storage.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private void Register(StorageImageFile image, string key, string message)
        {
            _ = eventRegister.RegisterEvent(image.TenantId, image.CreatedBy, key, message, new Dictionary<string, string>
            {
                { "ImageId", image.Id },
                { "FileName", image.OriginalFileName },
                { "ImageFileId", image.ImageId },
                { "HiResImage", image.HiResImageUrl },
                { "MidResImage", image.MidResImageUrl },
                { "LowResImage", image.LowResImageUrl }
            });
        }
    }
}