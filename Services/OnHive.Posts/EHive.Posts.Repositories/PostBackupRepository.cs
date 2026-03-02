using MongoDB.Driver;
using EHive.Core.Library.Entities.Posts;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Posts.Domain.Abstractions.Repositories;

namespace EHive.Posts.Repositories
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