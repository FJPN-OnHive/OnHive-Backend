using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Payments;
using System.Text.Json;

namespace EHive.Payments.Domain.Abstractions.Services
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