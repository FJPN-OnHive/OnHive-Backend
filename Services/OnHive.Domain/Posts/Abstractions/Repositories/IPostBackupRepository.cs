using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Posts;

namespace OnHive.Posts.Domain.Abstractions.Repositories
{
    public interface IPostBackupRepository : IRepositoryBase<BlogPostBackup>
    {
    }
}