using MongoDB.Driver;
using EHive.Core.Library.Entities.Messages;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Messages.Domain.Abstractions.Repositories;

namespace EHive.Messages.Repositories
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