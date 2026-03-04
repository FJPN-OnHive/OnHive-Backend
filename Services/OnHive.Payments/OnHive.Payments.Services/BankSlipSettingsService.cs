using AutoMapper;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Payments;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Payments.Domain.Abstractions.Repositories;
using OnHive.Payments.Domain.Abstractions.Services;
using Serilog;
using System.Text.Json;

namespace OnHive.Payments.Services
{
    public class BankSlipSettingsService : IBankSlipSettingsService
    {
        private readonly IBankSlipSettingsRepository bankSlipSettingsRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public BankSlipSettingsService(IBankSlipSettingsRepository bankSlipSettingsRepository, IMapper mapper)
        {
            this.bankSlipSettingsRepository = bankSlipSettingsRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<BankSlipSettingsDto> GetByIdAsync(string bankSlipSettingsId)
        {
            var bankSlipSettings = await bankSlipSettingsRepository.GetByIdAsync(bankSlipSettingsId);
            return mapper.Map<BankSlipSettingsDto>(bankSlipSettings);
        }

        public async Task<PaginatedResult<BankSlipSettingsDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await bankSlipSettingsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<BankSlipSettingsDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<BankSlipSettingsDto>>(result.Itens)
                };
            }
            return new PaginatedResult<BankSlipSettingsDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<BankSlipSettingsDto>()
            };
        }

        public async Task<IEnumerable<BankSlipSettingsDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var BankSlipSettingss = await bankSlipSettingsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<BankSlipSettingsDto>>(BankSlipSettingss);
        }

        public async Task<BankSlipSettingsDto> SaveAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto? loggedUser)
        {
            if (!bankSlipSettingsDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var bankSlipSettings = mapper.Map<BankSlipSettings>(bankSlipSettingsDto);
            ValidatePermissions(bankSlipSettings, loggedUser?.User);
            bankSlipSettings.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            bankSlipSettings.CreatedAt = DateTime.UtcNow;
            bankSlipSettings.CreatedBy = string.IsNullOrEmpty(bankSlipSettings.CreatedBy) ? loggedUser?.User?.Id : bankSlipSettings.CreatedBy;
            var response = await bankSlipSettingsRepository.SaveAsync(bankSlipSettings);
            return mapper.Map<BankSlipSettingsDto>(response);
        }

        public async Task<BankSlipSettingsDto> CreateAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto loggedUser)
        {
            if (!bankSlipSettingsDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var BankSlipSettings = mapper.Map<BankSlipSettings>(bankSlipSettingsDto);
            ValidatePermissions(BankSlipSettings, loggedUser?.User);
            BankSlipSettings.Id = string.Empty;
            BankSlipSettings.TenantId = loggedUser?.User?.TenantId;
            var response = await bankSlipSettingsRepository.SaveAsync(BankSlipSettings, loggedUser.User.Id);
            return mapper.Map<BankSlipSettingsDto>(response);
        }

        public async Task<BankSlipSettingsDto?> UpdateAsync(BankSlipSettingsDto bankSlipSettingsDto, LoggedUserDto loggedUser)
        {
            if (!bankSlipSettingsDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var BankSlipSettings = mapper.Map<BankSlipSettings>(bankSlipSettingsDto);
            ValidatePermissions(BankSlipSettings, loggedUser?.User);
            var currentBankSlipSettings = await bankSlipSettingsRepository.GetByIdAsync(BankSlipSettings.Id);
            if (currentBankSlipSettings == null || currentBankSlipSettings.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await bankSlipSettingsRepository.SaveAsync(BankSlipSettings, loggedUser.User.Id);
            return mapper.Map<BankSlipSettingsDto>(response);
        }

        public async Task<BankSlipSettingsDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentBankSlipSettings = await bankSlipSettingsRepository.GetByIdAsync(patch.GetId());
            if (currentBankSlipSettings == null || currentBankSlipSettings.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentBankSlipSettings = patch.PatchEntity(currentBankSlipSettings);
            ValidatePermissions(currentBankSlipSettings, loggedUser.User);
            if (!mapper.Map<BankSlipSettingsDto>(currentBankSlipSettings).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await bankSlipSettingsRepository.SaveAsync(currentBankSlipSettings, loggedUser.User.Id);
            return mapper.Map<BankSlipSettingsDto>(response);
        }

        private void ValidatePermissions(BankSlipSettings bankSlipSettings, UserDto? loggedUser)
        {
            if (loggedUser != null && bankSlipSettings.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID BankSlipSettings/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    bankSlipSettings.Id, bankSlipSettings.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}