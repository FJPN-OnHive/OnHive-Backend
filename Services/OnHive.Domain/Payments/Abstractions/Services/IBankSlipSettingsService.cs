using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Payments;
using System.Text.Json;

namespace OnHive.Payments.Domain.Abstractions.Services
{
    public interface IBankSlipSettingsService
    {
        Task<BankSlipSettingsDto?> GetByIdAsync(string bankSlipSettingsId);

        Task<PaginatedResult<BankSlipSettingsDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<BankSlipSettingsDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<BankSlipSettingsDto> SaveAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto? user);

        Task<BankSlipSettingsDto> CreateAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto loggedUser);

        Task<BankSlipSettingsDto?> UpdateAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto loggedUser);

        Task<BankSlipSettingsDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);
    }
}