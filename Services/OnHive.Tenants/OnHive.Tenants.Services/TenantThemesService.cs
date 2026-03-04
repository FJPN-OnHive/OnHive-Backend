using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Exceptions;
using OnHive.Tenants.Domain.Models;
using Serilog;
using System.Text.Json;

namespace OnHive.Tenants.Services
{
    public class TenantThemesService : ITenantThemesService
    {
        private readonly ITenantThemesRepository tenantThemesRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public TenantThemesService(ITenantThemesRepository tenantThemesRepository, TenantsApiSettings tenantsApiSettings, IMapper mapper)
        {
            this.tenantThemesRepository = tenantThemesRepository;
            this.tenantsApiSettings = tenantsApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<IEnumerable<TenantThemeDto>> GetByDomain(string domain, string tenantId)
        {
            var themes = await tenantThemesRepository.GetByDomain(domain, tenantId);
            themes.RemoveAll(themes => !themes.IsActive);
            return mapper.Map<IEnumerable<TenantThemeDto>>(themes);
        }

        public async Task<TenantThemeDto> GetCurrentByDomain(string domain, string tenantId)
        {
            var themes = await tenantThemesRepository.GetByDomain(domain, tenantId);
            var baseTheme = themes.Find(t => t.IsBaseStyle && t.IsActive);
            var currentTheme = themes.Find(t => t.StartDate <= DateTime.UtcNow && t.EndDate >= DateTime.UtcNow && t.IsActive);
            if (currentTheme == null)
            {
                return mapper.Map<TenantThemeDto>(baseTheme);
            }
            return mapper.Map<TenantThemeDto>(currentTheme);
        }

        public async Task<TenantThemeDto?> GetByIdAsync(string tenantParameterId)
        {
            var tenantParameter = await tenantThemesRepository.GetByIdAsync(tenantParameterId);
            return mapper.Map<TenantThemeDto>(tenantParameter);
        }

        public async Task<PaginatedResult<TenantThemeDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await tenantThemesRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<TenantThemeDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<TenantThemeDto>>(result.Itens)
                };
            }
            return new PaginatedResult<TenantThemeDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<TenantThemeDto>()
            };
        }

        public async Task<IEnumerable<TenantThemeDto>> GetAllAsync(UserDto? loggedUser)
        {
            var tenantParameters = await tenantThemesRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<TenantThemeDto>>(tenantParameters);
        }

        public async Task<TenantThemeDto> SaveAsync(TenantThemeDto tenantParameterDto, UserDto? loggedUser)
        {
            var tenantParameter = mapper.Map<TenantTheme>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            tenantParameter.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            tenantParameter.CreatedAt = DateTime.UtcNow;
            tenantParameter.CreatedBy = string.IsNullOrEmpty(tenantParameter.CreatedBy) ? loggedUser.Id : tenantParameter.CreatedBy;
            await VerifyDuplicated(tenantParameter);
            var response = await tenantThemesRepository.SaveAsync(tenantParameter);
            return mapper.Map<TenantThemeDto>(response);
        }

        public async Task<TenantThemeDto> CreateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser)
        {
            var tenantParameter = mapper.Map<TenantTheme>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            tenantParameter.Id = string.Empty;
            tenantParameter.TenantId = loggedUser.TenantId;
            await VerifyDuplicated(tenantParameter);
            var response = await tenantThemesRepository.SaveAsync(tenantParameter, loggedUser.Id);
            return mapper.Map<TenantThemeDto>(response);
        }

        public async Task<TenantThemeDto?> UpdateAsync(TenantThemeDto tenantParameterDto, UserDto loggedUser)
        {
            var tenantParameter = mapper.Map<TenantTheme>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            var currentTenantParameter = await tenantThemesRepository.GetByIdAsync(tenantParameter.Id);
            if (currentTenantParameter == null || currentTenantParameter.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            await VerifyDuplicated(tenantParameter);
            var response = await tenantThemesRepository.SaveAsync(tenantParameter, loggedUser.Id);
            return mapper.Map<TenantThemeDto>(response);
        }

        public async Task<TenantThemeDto?> PatchAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentExam = await tenantThemesRepository.GetByIdAsync(patch.GetId());
            if (currentExam == null || currentExam.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentExam = patch.PatchEntity(currentExam);
            if (!mapper.Map<TenantThemeDto>(currentExam).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentExam, loggedUser);
            currentExam.IsActive = true;
            var response = await tenantThemesRepository.SaveAsync(currentExam, loggedUser.Id);
            return mapper.Map<TenantThemeDto>(response);
        }

        private async Task VerifyDuplicated(TenantTheme tenantTheme)
        {
            var current = await tenantThemesRepository.GetByDomain(tenantTheme.Domain, tenantTheme.TenantId);
            if (current != null && current.Exists(c => c.IsActive && c.IsBaseStyle && tenantTheme.IsBaseStyle && c.Id != tenantTheme.Id))
            {
                throw new DuplicatedParameterException();
            }
        }

        private void ValidatePermissions(TenantTheme tenantParameter, UserDto? loggedUser)
        {
            if (loggedUser != null
                && !loggedUser.Permissions.Exists(p => p.Equals(tenantsApiSettings.TenantsAdminPermission, StringComparison.InvariantCultureIgnoreCase)))
            {
                logger.Warning("Unauthorized update: {id}, logged user: {loggedUserId}", tenantParameter, loggedUser.Id);
                throw new UnauthorizedAccessException();
            }

            if (loggedUser != null && tenantParameter.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID TenantParameter/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    tenantParameter.Id, tenantParameter.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}