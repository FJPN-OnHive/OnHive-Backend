using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Payments;

namespace EHive.Payments.Domain.Abstractions.Repositories
{
    public interface IBankSlipNumberControlRepository : IRepositoryBase<BankSlipNumberControl>
    {
        public BankSlipNumberControl GetByProvider(string providerKey);

        public Task<int> GetNextAsync(string providerKey, string orderId, string paymentId);
    }
}