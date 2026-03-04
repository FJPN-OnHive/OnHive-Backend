using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Videos;

namespace OnHive.Videos.Domain.Abstractions.Repositories
{
    public interface IVideosRepository : IRepositoryBase<Video>
    {
        Task<Video?> GetByVideoIdAsync(string id);
        Task<List<Video>> GetPendingVideosAsync();
    }
}