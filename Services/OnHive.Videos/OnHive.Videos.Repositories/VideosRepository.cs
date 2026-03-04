using MongoDB.Driver;
using OnHive.Core.Library.Entities.Videos;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Videos.Domain.Abstractions.Repositories;
using SharpCompress.Common;
using System.Linq.Expressions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Enums.Videos;

namespace OnHive.Videos.Repositories
{
    public class VideosRepository : MongoRepositoryBase<Video>, IVideosRepository
    {
        public VideosRepository(MongoDBSettings settings) : base(settings, "Videos")
        {
        }

        public async Task<Video?> GetByVideoIdAsync(string id)
        {
            var filter = Builders<Video>.Filter.Eq(x => x.VideoId, id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Video>> GetPendingVideosAsync()
        {
            var filter = Builders<Video>.Filter.Eq(x => x.Status, Core.Library.Enums.Videos.VideoStatus.Uploading);
            return await collection.Find(filter).ToListAsync();
        }

        public override Task<PaginatedResult<Video>> GetByFilterAsync(RequestFilter filter, string? tenantId, bool activeOnly = true)
        {
            filter.AndFilter.Add(new FilterField { Field = "Status", Operator = "nin", Value = "3" });
            return base.GetByFilterAsync(filter, tenantId, activeOnly);
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Video>(Builders<Video>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Video>(Builders<Video>.IndexKeys.Ascending(i => i.VideoId)));
        }
    }
}