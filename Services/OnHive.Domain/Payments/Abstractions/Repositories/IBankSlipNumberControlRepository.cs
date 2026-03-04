using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Payments;

namespace OnHive.Payments.Domain.Abstractions.Repositories
{
    public interface IBankSlipNumberControlRepository : IRepositoryBase<BankSlipNumberControl>
    {
        public BankSlipNumberControl GetByProvider(string providerKey);

        public Task<int> GetNextAsync(string providerKey, string orderId, string paymentId);
    }
}