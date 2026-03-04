using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Entities.Payments;

namespace OnHive.Payments.Domain.Abstractions.Repositories
{
    public interface IBankSlipSettingsRepository : IRepositoryBase<BankSlipSettings>
    {
        Task<BankSlipSettings?> GetByProviderAsync(string bankSlipProviderKey);
    }
}