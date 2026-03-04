using MongoDB.Driver;
using OnHive.Core.Library.Entities.Messages;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Messages.Domain.Abstractions.Repositories;

namespace OnHive.Messages.Repositories
{
    public class MessageChannelsRepository : MongoRepositoryBase<MessageChannel>, IMessageChannelRepository
    {
        public MessageChannelsRepository(MongoDBSettings settings) : base(settings, "MessageChannels")
        {
        }

        public Task<MessageChannel> GetByCodeAsync(string channelCode, string tenantId)
        {
            var filter = Builders<MessageChannel>.Filter.Eq(i => i.Code, channelCode)
                            & Builders<MessageChannel>.Filter.Eq(i => i.TenantId, tenantId);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<MessageChannel>(Builders<MessageChannel>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<MessageChannel>(Builders<MessageChannel>.IndexKeys.Ascending(i => i.Code)));
        }
    }
}