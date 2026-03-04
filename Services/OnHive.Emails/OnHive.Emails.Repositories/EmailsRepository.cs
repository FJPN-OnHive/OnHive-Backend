using OnHive.Core.Library.Entities.Emails;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Emails.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace OnHive.Emails.Repositories
{
    public class EmailsRepository : MongoRepositoryBase<EmailTemplate>, IEmailsRepository
    {
        public EmailsRepository(MongoDBSettings settings) : base(settings, "Emails")
        {
        }

        public async Task<EmailTemplate> GetByCodeAsync(string templateCode, string tenantId)
        {
            var filter = Builders<EmailTemplate>.Filter.Eq(e => e.Code, templateCode)
                & Builders<EmailTemplate>.Filter.Eq(e => e.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<EmailTemplate>(Builders<EmailTemplate>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<EmailTemplate>(Builders<EmailTemplate>.IndexKeys.Ascending(i => i.Code)));
        }
    }
}