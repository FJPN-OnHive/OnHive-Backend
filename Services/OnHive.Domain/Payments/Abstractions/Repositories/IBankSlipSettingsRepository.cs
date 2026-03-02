using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Entities.Payments;

namespace EHive.Payments.Domain.Abstractions.Repositories
{
    public interface IBankSlipSettingsRepository : IRepositoryBase<BankSlipSettings>
    {
        Task<BankSlipSettings?> GetByProviderAsync(string bankSlipProviderKey);
    }
}