using MongoDB.Driver;
using OnHive.Core.Library.Entities.Users;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Users.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Enums.Users;
using OnHive.Core.Library.Helpers;
using SharpCompress.Common;

namespace OnHive.Users.Repositories
{
    public class UserProfilesRepository : MongoRepositoryBase<UserProfile>, IUserProfilesRepository
    {
        public UserProfilesRepository(MongoDBSettings settings) : base(settings, "UserProfiles")
        {
        }

        public async Task<PaginatedResult<UserProfile>> GetByFilterAndTypeAsync(RequestFilter filter, ProfileTypes type, string tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<UserProfile>(filter, tenantId, true);

            if (type != ProfileTypes.None)
            {
                queryFilter = Builders<UserProfile>.Filter.And(queryFilter, Builders<UserProfile>.Filter.Eq(u => u.Type, type));
            }

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<UserProfile>(filter));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<UserProfile>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        public async Task<List<UserProfile>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<UserProfile>(Builders<UserProfile>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<UserProfile>(Builders<UserProfile>.IndexKeys.Ascending(i => i.UserId)));
            collection.Indexes.CreateOne(new CreateIndexModel<UserProfile>(Builders<UserProfile>.IndexKeys.Ascending(i => i.Type)));
            collection.Indexes.CreateOne(new CreateIndexModel<UserProfile>(Builders<UserProfile>.IndexKeys.Text(i => i.Title)));
        }
    }
}