using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Videos;
using OnHive.Core.Library.Entities.Videos;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Validations.Common;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Videos.Domain.Abstractions.Repositories;
using OnHive.Videos.Domain.Abstractions.Services;
using OnHive.Videos.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using Mux.Csharp.Sdk.Api;
using Mux.Csharp.Sdk.Model;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace OnHive.Videos.Services
{
    public class VideosService : IVideosService
    {
        private readonly IVideosRepository videosRepository;
        private readonly VideosApiSettings videosApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEventRegister eventRegister;

        public VideosService(IVideosRepository videosRepository, VideosApiSettings videosApiSettings, IMapper mapper, IEventRegister eventRegister)
        {
            this.videosRepository = videosRepository;
            this.videosApiSettings = videosApiSettings;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<VideoDto?> GetByIdAsync(string videoId)
        {
            var video = await videosRepository.GetByIdAsync(videoId);
            return mapper.Map<VideoDto>(video);
        }

        public async Task<PaginatedResult<VideoDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await videosRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<VideoDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<VideoDto>>(result.Itens)
                };
            }
            return new PaginatedResult<VideoDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<VideoDto>()
            };
        }

        public async Task<IEnumerable<VideoDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var videos = await videosRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<VideoDto>>(videos);
        }

        public async Task<VideoDto> SaveAsync(VideoDto videoDto, LoggedUserDto? loggedUser)
        {
            var video = mapper.Map<Video>(videoDto);
            ValidatePermissions(video, loggedUser?.User);
            video.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            video.IsActive = true;
            video.CreatedAt = DateTime.UtcNow;
            video.CreatedBy = string.IsNullOrEmpty(video.CreatedBy) ? loggedUser?.User?.Id : video.CreatedBy;
            var response = await videosRepository.SaveAsync(video);
            return mapper.Map<VideoDto>(response);
        }

        public async Task<VideoDto> CreateAsync(VideoDto videoDto, LoggedUserDto? loggedUser)
        {
            if (!videoDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var video = mapper.Map<Video>(videoDto);
            ValidatePermissions(video, loggedUser?.User);
            video.Id = string.Empty;
            video.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            video.IsActive = true;
            var response = await videosRepository.SaveAsync(video, loggedUser.User.Id);
            return mapper.Map<VideoDto>(response);
        }

        public async Task<VideoDto?> UpdateAsync(VideoDto videoDto, LoggedUserDto? loggedUser)
        {
            if (!videoDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var video = mapper.Map<Video>(videoDto);
            ValidatePermissions(video, loggedUser?.User);
            var currentVideo = await videosRepository.GetByIdAsync(video.Id);
            if (currentVideo == null || currentVideo.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await videosRepository.SaveAsync(video, loggedUser.User.Id);
            return mapper.Map<VideoDto>(response);
        }

        private void ValidatePermissions(Video video, UserDto? loggedUser)
        {
            if (loggedUser != null && video.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Video/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    video.Id, video.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        public async Task<string> GetVideoUrlAsync(string videoId, LoggedUserDto? loggedUser)
        {
            var video = await videosRepository.GetByIdAsync(videoId);
            if (video == null || video.TenantId != loggedUser.User.TenantId)
            {
                throw new NotFoundException($"Video {videoId} not found");
            }
            if (video.Status == Core.Library.Enums.Videos.VideoStatus.Error)
            {
                throw new NotFoundException($"Video {videoId} Upload error");
            }
            return video.VideoSource.ToUpper() switch
            {
                "MUX" => await GetMuxMuxVideoUrl(video, loggedUser),
                _ => throw new NotSupportedException($"Video source {video.VideoSource} is not supported.")
            };
        }

        public async Task<VideoUploadDto?> GetUploadUrlAsync(VideoDto videoDto, LoggedUserDto? loggedUser)
        {
            logger.Information("Starting video upload for file: {fileName}, user: {userId}", videoDto.FileName, loggedUser?.User?.Id);
            var video = mapper.Map<Video>(videoDto);
            video.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            video.Status = Core.Library.Enums.Videos.VideoStatus.Uploading;
            video.CreatedAt = DateTime.UtcNow;
            video.CreatedBy = loggedUser?.User?.Id ?? string.Empty;
            video = await videosRepository.SaveAsync(video, loggedUser.User.Id);
            var result = video.VideoSource.ToUpper() switch
            {
                "MUX" => await MuxUploadLinkAsync(video),
                _ => throw new NotSupportedException($"Video source {video.VideoSource} is not supported.")
            };
            video = await videosRepository.SaveAsync(video, loggedUser.User.Id);
            result.VideoRegistry = mapper.Map<VideoDto>(video);
            Register(video!, "Video.Upload.Registered", "Video upload registered.");
            logger.Information("Video upload registered with id: {videoId} for file: {fileName}, user: {userId}", video.Id, videoDto.FileName, loggedUser?.User?.Id);
            return result;
        }

        public async Task<string> CheckProcessingVideos()
        {
            var result = string.Empty;
            var pendingVideos = await videosRepository.GetPendingVideosAsync();
            foreach (var item in pendingVideos)
            {
                if (item.CreatedAt <= DateTime.UtcNow.AddDays(-1))
                {
                    logger.Information($"Video Upload Expired : {item.Id}");
                    item.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                    await videosRepository.SaveAsync(item);
                    continue;
                }
                switch (item.VideoSource.ToUpper())
                {
                    case "MUX":
                        await CheckMuxUpload(item);
                        break;

                    default:
                        item.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                        await videosRepository.SaveAsync(item);
                        logger.Error($"Video source {item.VideoSource}, video {item.Id} is not supported.");
                        break;
                }
            }
            return result;
        }

        private async Task CheckMuxUpload(Video item)
        {
            var config = new Mux.Csharp.Sdk.Client.Configuration();
            config.BasePath = videosApiSettings.MuxApi;
            config.Username = videosApiSettings.MuxUser;
            config.Password = videosApiSettings.MuxPassword;
            var apiInstance = new DirectUploadsApi(config);
            var response = await apiInstance.GetDirectUploadAsync(item.SourceId);
            if (response != null)
            {
                if (response.Data.Status == Upload.StatusEnum.AssetCreated)
                {
                    item.VideoId = response.Data.AssetId;
                    item.Status = Core.Library.Enums.Videos.VideoStatus.Ready;
                    await videosRepository.SaveAsync(item);
                    logger.Information($"Mux video upload ready: AssetId {item.VideoId}, video {item.Id}.");
                }
                else if (response.Data.Status == Upload.StatusEnum.Errored || response.Data.Status == Upload.StatusEnum.TimedOut)
                {
                    item.VideoId = response.Data.AssetId;
                    item.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                    await videosRepository.SaveAsync(item);
                    logger.Error($"Error on mux video upload: {response.Data.Error}, video {item.Id}.");
                }
            }
            else
            {
                item.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                await videosRepository.SaveAsync(item);
                logger.Error($"Error on mux video upload: Upload not found, video {item.Id}.");
            }
        }

        private async Task<string> GetMuxMuxVideoUrl(Video video, LoggedUserDto? loggedUser)
        {
            try
            {
                if (loggedUser == null || loggedUser.User == null || video.TenantId != loggedUser.User.TenantId)
                {
                    throw new UnauthorizedAccessException($"Invalid TenantId, user: {loggedUser.User.Id} - Tenant : {loggedUser.User.TenantId}");
                }
                if (loggedUser.User.Tenant == null || loggedUser.User.Tenant.Features == null || !loggedUser.User.Tenant.Features.Any(f => f.Equals("MUX", StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new UnauthorizedAccessException($"Invalid Feature MUX, user: {loggedUser.User.Id} - Tenant : {loggedUser.User.TenantId}");
                }
                if (video.Status != Core.Library.Enums.Videos.VideoStatus.Ready)
                {
                    await CheckMuxUpload(video);
                    if (video.Status != Core.Library.Enums.Videos.VideoStatus.Ready)
                    {
                        throw new InvalidOperationException($"Video {video.Id} is not ready. Current status: {video.Status}");
                    }
                }
                var config = new Mux.Csharp.Sdk.Client.Configuration();
                config.BasePath = videosApiSettings.MuxApi;
                config.Username = videosApiSettings.MuxUser;
                config.Password = videosApiSettings.MuxPassword;
                var apiInstance = new AssetsApi(config);
                var response = await apiInstance.GetAssetAsync(video.VideoId);
                if (response != null)
                {
                    if (response.Data.Meta.CreatorId != loggedUser.User.TenantId)
                    {
                        throw new UnauthorizedAccessException($"Invalid TenantId for video {video.Id}, user: {loggedUser.User.Id} - Tenant : {loggedUser.User.TenantId}");
                    }
                    var playbackId = response.Data.PlaybackIds.FirstOrDefault();
                    return GenerateMuxSignedPlaybackUrl(playbackId.Id);
                }
                else
                {
                    video.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                    await videosRepository.SaveAsync(video);
                    logger.Error($"Error on mux video: Asset not found, video {video.Id}.");
                }
            }
            catch (Exception ex)
            {
                video.Status = Core.Library.Enums.Videos.VideoStatus.Error;
                Log.Error(ex, "Error getting video url from mux, video {id}", video.Id);
                await videosRepository.SaveAsync(video);
                throw new NotFoundException($"Video not found on Mux: {video.Id}");
            }
            return string.Empty;
        }

        public string GenerateMuxSignedPlaybackUrl(string playbackId)
        {
            var credentials = new SigningCredentials(LoadPrivateKey(videosApiSettings.MuxSigningSecretKey), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var claims = new[]
            {
                new Claim("kid", videosApiSettings.MuxSigningKeyId),
                new Claim("aud", "v"),
                new Claim("sub", playbackId)
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            return $"https://stream.mux.com/{playbackId}.m3u8?token={jwt}";
        }

        private RsaSecurityKey LoadPrivateKey(string privateKey)
        {
            try
            {
                var rsa = RSA.Create();

                if (privateKey.Contains("BEGIN PRIVATE KEY"))
                {
                    // PKCS#8 format
                    rsa.ImportFromPem(privateKey);
                }
                else if (privateKey.Contains("BEGIN RSA PRIVATE KEY"))
                {
                    // PKCS#1 format
                    rsa.ImportFromPem(privateKey);
                }
                else
                {
                    // Assume it's base64 without headers
                    var keyBytes = Convert.FromBase64String(privateKey);

                    // Try different formats
                    try
                    {
                        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                    }
                    catch
                    {
                        try
                        {
                            rsa.ImportRSAPrivateKey(keyBytes, out _);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Formato de chave privada não suportado");
                        }
                    }
                }

                return new RsaSecurityKey(rsa);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Falha ao carregar a chave privada do Mux", ex);
            }
        }

        private async Task<VideoUploadDto> MuxUploadLinkAsync(Video video)
        {
            var result = new VideoUploadDto();
            var config = new Mux.Csharp.Sdk.Client.Configuration();
            config.BasePath = videosApiSettings.MuxApi;
            config.Username = videosApiSettings.MuxUser;
            config.Password = videosApiSettings.MuxPassword;
            var apiInstance = new DirectUploadsApi(config);
            var createUploadRequest = new CreateUploadRequest(newAssetSettings: new CreateAssetRequest
            {
                VideoQuality = CreateAssetRequest.VideoQualityEnum.Basic,
                PlaybackPolicies = [PlaybackPolicy.Signed],
                Meta = new AssetMetadata
                {
                    Title = video.Name,
                    ExternalId = video.Id,
                    CreatorId = video.TenantId
                },
                Test = videosApiSettings.MuxEnv.Trim().ToUpper().Equals("DEVELOPMENT")
            }, corsOrigin: videosApiSettings.MuxCors);
            var response = await apiInstance.CreateDirectUploadAsync(createUploadRequest);
            result.UploadUrl = response.Data.Url;
            video.SourceId = response.Data.Id;
            return result;
        }

        private void Register(Video video, string key, string message)
        {
            _ = eventRegister.RegisterEvent(video.TenantId, video.CreatedBy, key, message, new Dictionary<string, string>
            {
                { "Id", video.Id },
                { "FileName", video.FileName },
                { "VideoId", video.VideoId },
                { "VideoUrl", video.VideoUrl },
                { "SourceVideoUrl", video.SourceFileUrl },
                { "VideoStatus", video.Status.ToString() }
            });
        }
    }
}