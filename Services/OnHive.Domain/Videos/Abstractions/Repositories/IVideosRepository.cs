using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Videos;

namespace EHive.Videos.Domain.Abstractions.Repositories
{
    public interface IVideosRepository : IRepositoryBase<Video>
    {
        Task<Video?> GetByVideoIdAsync(string id);
        Task<List<Video>> GetPendingVideosAsync();
    }
}