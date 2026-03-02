using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Posts;

namespace EHive.Posts.Domain.Abstractions.Repositories
{
    public interface IPostBackupRepository : IRepositoryBase<BlogPostBackup>
    {
    }
}