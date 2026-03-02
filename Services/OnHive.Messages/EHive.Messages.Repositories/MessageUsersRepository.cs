using MongoDB.Driver;
using EHive.Core.Library.Entities.Messages;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Messages.Domain.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Helpers;

namespace EHive.Messages.Repositories
{
    public class MessageUsersRepository : MongoRepositoryBase<MessageUser>, IMessageUsersRepository
    {
        public MessageUsersRepository(MongoDBSettings settings) : base(settings, "MessageUsers")
        {
        }

        public async Task<IEnumerable<MessageUser>> GetByMessageAsync(string messageId, string tenantId)
        {
            var filter = Builders<MessageUser>.Filter.Eq(i => i.MessageId, messageId)
                & Builders<MessageUser>.Filter.Eq(i => i.TenantId, tenantId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<MessageUser>> GetByUserAsync(string userId, bool newOnly, string tenantId)
        {
            var filter = Builders<MessageUser>.Filter.Eq(i => i.UserId, userId)
                & Builders<MessageUser>.Filter.Eq(i => i.TenantId, tenantId)
                & Builders<MessageUser>.Filter.Ne(i => i.Status, Core.Library.Enums.Messages.MessageStatus.Deleted);
            if (newOnly)
            {
                filter &= Builders<MessageUser>.Filter.Eq(i => i.Status, Core.Library.Enums.Messages.MessageStatus.New);
            }
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<PaginatedResult<MessageUser>> GetByUserFilterAsync(RequestFilter filter, string userId, string tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<MessageUser>(filter, tenantId, false)
                 & Builders<MessageUser>.Filter.Eq(e => e.UserId, userId);
            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<MessageUser>(filter));
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

            return new PaginatedResult<MessageUser>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<MessageUser>(Builders<MessageUser>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<MessageUser>(Builders<MessageUser>.IndexKeys.Ascending(i => i.UserId)));
            collection.Indexes.CreateOne(new CreateIndexModel<MessageUser>(Builders<MessageUser>.IndexKeys.Ascending(i => i.MessageId)));
        }
    }
}