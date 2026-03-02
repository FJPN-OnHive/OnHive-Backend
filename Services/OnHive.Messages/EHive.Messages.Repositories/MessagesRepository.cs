using MongoDB.Driver;
using EHive.Core.Library.Entities.Messages;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Messages.Domain.Abstractions.Repositories;

namespace EHive.Messages.Repositories
{
    public class MessagesRepository : MongoRepositoryBase<Message>, IMessagesRepository
    {
        public MessagesRepository(MongoDBSettings settings) : base(settings, "Messages")
        {
        }

        public Task<List<Message>> GetByFromAsync(string from, string origin, string tenantId)
        {
            var filter = Builders<Message>.Filter.Eq(i => i.From.Email, from) &
                         Builders<Message>.Filter.Eq(i => i.Origin, origin) &
                         Builders<Message>.Filter.Eq(i => i.TenantId, tenantId);

            return collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(i => i.ChannelId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(i => i.Code)));
        }
    }
}