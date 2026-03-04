using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Payments;

namespace OnHive.Payments.Domain.Abstractions.Repositories
{
    public interface IPaymentsRepository : IRepositoryBase<Payment>
    {
        Task<Payment> GetByOrderIdAsync(string orderId, string tenantId);

        Task<List<Payment>> GetByProviderAsync(string providerKey, string tenantId);

        Task<List<Payment>> GetByUserIdAsync(string userId, string tenantId);
    }
}