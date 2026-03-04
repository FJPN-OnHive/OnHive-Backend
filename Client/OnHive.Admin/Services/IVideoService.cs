using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Contracts.Videos;

namespace OnHive.Admin.Services
{
    public interface IVideoService : IServiceBase<VideoDto>
    {
        Task<List<VideoDto>> GetVideosAsync(string token);
                   
        Task<VideoDto> CreateVideoAsync(VideoDto file, Stream video, string token);
        
        Task<VideoDto> UpdateVideoAsync(VideoDto file, Stream videoStream, string token);
    }
}
