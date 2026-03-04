using MongoDB.Driver;
using OnHive.Core.Library.Entities.Posts;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Posts.Domain.Abstractions.Repositories;

namespace OnHive.Posts.Repositories
{
    public class PostBackupRepository : MongoRepositoryBase<BlogPostBackup>, IPostBackupRepository
    {
        public PostBackupRepository(MongoDBSettings settings) : base(settings, "PostBackup")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPostBackup>(Builders<BlogPostBackup>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPostBackup>(Builders<BlogPostBackup>.IndexKeys.Ascending(i => i.PostId)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPostBackup>(Builders<BlogPostBackup>.IndexKeys.Ascending(i => i.SnapShotDate)));
        }
    }
}