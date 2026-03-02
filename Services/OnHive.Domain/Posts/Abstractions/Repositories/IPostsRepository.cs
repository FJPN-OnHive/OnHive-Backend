using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Posts;

namespace EHive.Posts.Domain.Abstractions.Repositories
{
    public interface IPostsRepository : IRepositoryBase<BlogPost>
    {
        Task<BlogPost> GetBySlug(string slug, string tenantId);

        Task<PaginatedResult<BlogPost>> GetPublishedByFilterAsync(RequestFilter filter, string? tenantId, bool publicOnly);

        Task<List<BlogPost>> GetPublishedByCourseAsync(string courseId, string? tenantId);

        Task<long> DeleteUnsaved(DateTime olderThan);

        Task<List<BlogPost>> GetReadyToPublish();

        Task<FilterScope> GetFilterDataAsync(string tenantId);

        Task<List<string>> GetSlugsAsync(string tenantId);

        Task<List<BlogPost>> GetAllActive(string tenantId);

        Task<List<BlogPost>> GetPublishedCanonicalListAsync(string tenantId);

        Task<BlogPost?> GetByAlternativeSlug(string slug, string tenantId);
    }
}