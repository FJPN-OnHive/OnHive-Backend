using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
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
    public class TenantParametersService : ITenantParametersService
    {
        private readonly ITenantParametersRepository tenantParametersRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public TenantParametersService(ITenantParametersRepository tenantParametersRepository, TenantsApiSettings tenantsApiSettings, IMapper mapper)
        {
            this.tenantParametersRepository = tenantParametersRepository;
            this.tenantsApiSettings = tenantsApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<IEnumerable<TenantParameterDto>> GetByGroup(string group, string tenantId)
        {
            var result = await tenantParametersRepository.GetByGroup(group, tenantId);
            return mapper.Map<List<TenantParameterDto>>(result);
        }

        public async Task<TenantParameterDto?> GetByKey(string group, string key, string tenantId)
        {
            var result = await tenantParametersRepository.GetByKey(group, key, tenantId);
            return mapper.Map<TenantParameterDto>(result);
        }

        public async Task<TenantParameterDto?> GetByIdAsync(string tenantParameterId)
        {
            var tenantParameter = await tenantParametersRepository.GetByIdAsync(tenantParameterId);
            return mapper.Map<TenantParameterDto>(tenantParameter);
        }

        public async Task<PaginatedResult<TenantParameterDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await tenantParametersRepository.GetByFilterAsync(filter, loggedUser?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<TenantParameterDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<TenantParameterDto>>(result.Itens)
                };
            }
            return new PaginatedResult<TenantParameterDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<TenantParameterDto>()
            };
        }

        public async Task<IEnumerable<TenantParameterDto>> GetAllAsync(UserDto? loggedUser)
        {
            var tenantParameters = await tenantParametersRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<TenantParameterDto>>(tenantParameters);
        }

        public async Task<TenantParameterDto> SaveAsync(TenantParameterDto tenantParameterDto, UserDto? loggedUser)
        {
            var tenantParameter = mapper.Map<TenantParameter>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            tenantParameter.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            tenantParameter.CreatedAt = DateTime.UtcNow;
            tenantParameter.CreatedBy = string.IsNullOrEmpty(tenantParameter.CreatedBy) ? loggedUser.Id : tenantParameter.CreatedBy;
            await VerifyDuplicated(tenantParameter);
            var response = await tenantParametersRepository.SaveAsync(tenantParameter);
            return mapper.Map<TenantParameterDto>(response);
        }

        public async Task<TenantParameterDto> CreateAsync(TenantParameterDto tenantParameterDto, UserDto loggedUser)
        {
            var tenantParameter = mapper.Map<TenantParameter>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            tenantParameter.Id = string.Empty;
            tenantParameter.TenantId = loggedUser.TenantId;
            await VerifyDuplicated(tenantParameter);
            var response = await tenantParametersRepository.SaveAsync(tenantParameter, loggedUser.Id);
            return mapper.Map<TenantParameterDto>(response);
        }

        public async Task<TenantParameterDto?> UpdateAsync(TenantParameterDto tenantParameterDto, UserDto loggedUser)
        {
            var tenantParameter = mapper.Map<TenantParameter>(tenantParameterDto);
            ValidatePermissions(tenantParameter, loggedUser);
            var currentTenantParameter = await tenantParametersRepository.GetByIdAsync(tenantParameter.Id);
            if (currentTenantParameter == null || currentTenantParameter.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            await VerifyDuplicated(tenantParameter);
            var response = await tenantParametersRepository.SaveAsync(tenantParameter, loggedUser.Id);
            return mapper.Map<TenantParameterDto>(response);
        }

        public async Task<TenantParameterDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentStudent = await tenantParametersRepository.GetByIdAsync(patch.GetId());
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.TenantId)
            {
                return null;
            }
            currentStudent = patch.PatchEntity(currentStudent);
            ValidatePermissions(currentStudent, loggedUser);
            if (!mapper.Map<TenantParameterDto?>(currentStudent).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await tenantParametersRepository.SaveAsync(currentStudent, loggedUser.Id);
            return mapper.Map<TenantParameterDto>(response);
        }

        private async Task VerifyDuplicated(TenantParameter tenantParameter)
        {
            var current = await tenantParametersRepository.GetByKey(tenantParameter.Group, tenantParameter.Key, tenantParameter.TenantId);
            if (current != null)
            {
                throw new DuplicatedParameterException();
            }
        }

        private void ValidatePermissions(TenantParameter tenantParameter, UserDto? loggedUser)
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