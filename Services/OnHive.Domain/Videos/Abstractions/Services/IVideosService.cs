using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Videos;
using EHive.Core.Library.Contracts.Login;

namespace EHive.Videos.Domain.Abstractions.Services
{
    public interface IVideosService
    {
        Task<VideoDto?> GetByIdAsync(string videoId);

        Task<PaginatedResult<VideoDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<VideoDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<VideoDto> SaveAsync(VideoDto videoDto, LoggedUserDto? user);

        Task<VideoDto> CreateAsync(VideoDto videoDto, LoggedUserDto? loggedUser);

        Task<VideoDto?> UpdateAsync(VideoDto videoDto, LoggedUserDto? loggedUser);

        Task<VideoUploadDto?> GetUploadUrlAsync(VideoDto videoDto, LoggedUserDto? loggedUser);

        Task<string> GetVideoUrlAsync(string videoId, LoggedUserDto? loggedUser);

        Task<string> CheckProcessingVideos();
    }
}