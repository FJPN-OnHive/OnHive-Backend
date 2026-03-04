using MongoDB.Driver;
using OnHive.Core.Library.Entities.Invoices;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Invoices.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Enums.Invoices;

namespace OnHive.Invoices.Repositories
{
    public class InvoicesRepository : MongoRepositoryBase<Invoice>, IInvoicesRepository
    {
        public InvoicesRepository(MongoDBSettings settings) : base(settings, "Invoices")
        {
        }

        public async Task<int> GetLastInvoiceNumber(string tenantId, string invoiceSeries)
        {
            var filter = Builders<Invoice>.Filter.Eq(i => i.TenantId, tenantId)
                & Builders<Invoice>.Filter.Eq(i => i.Series, invoiceSeries);
            return await collection
                .Find(filter)
                .SortByDescending(i => i.Number)
                .Limit(1)
                .Project(i => i.Number)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Invoice>> GetPendingInvoices()
        {
            var filter = Builders<Invoice>.Filter.Eq(i => i.Status, InvoiceStatus.Pending);
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Invoice>(Builders<Invoice>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}