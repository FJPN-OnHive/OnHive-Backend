using AutoMapper;
using EHive.Core.Library.Contracts.Dict;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Dict;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Dict.Domain.Abstractions.Repositories;
using EHive.Dict.Domain.Abstractions.Services;
using EHive.Dict.Domain.Models;
using Serilog;

namespace EHive.Dict.Services
{
    public class DictService : IDictService
    {
        private readonly IDictRepository dictRepository;
        private readonly DictApiSettings dictApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public DictService(IDictRepository dictRepository, DictApiSettings dictApiSettings, IMapper mapper)
        {
            this.dictRepository = dictRepository;
            this.dictApiSettings = dictApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<ValueRegistryDto?> GetByIdAsync(string valuesId)
        {
            var values = await dictRepository.GetByIdAsync(valuesId);
            return mapper.Map<ValueRegistryDto>(values);
        }

        public async Task<ValueRegistryDto> GetByGroupAndKeyAsync(string tenantId, string group, string key)
        {
            var result = await dictRepository.GetByGroupAndKeyAsync(tenantId, group, key);
            return mapper.Map<ValueRegistryDto>(result);
        }

        public async Task<PaginatedResult<ValueRegistryDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await dictRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<ValueRegistryDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<ValueRegistryDto>>(result.Itens)
                };
            }
            return new PaginatedResult<ValueRegistryDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<ValueRegistryDto>()
            };
        }

        public async Task<IEnumerable<ValueRegistryDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var valuess = await dictRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<ValueRegistryDto>>(valuess);
        }

        public async Task<List<string>> GetGroupsAsync(string tenantId)
        {
            return await dictRepository.GetGroupsAsync(tenantId);
        }

        public async Task<List<string>> GetKeysAsync(string tenantId, string group)
        {
            return await dictRepository.GetKeysAsync(tenantId, group);
        }

        public async Task<ValueRegistryDto> SaveAsync(ValueRegistryDto valuesDto, LoggedUserDto? loggedUser)
        {
            var values = mapper.Map<ValueRegistry>(valuesDto);
            ValidatePermissions(values, loggedUser?.User);
            values.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            values.CreatedAt = DateTime.UtcNow;
            values.CreatedBy = string.IsNullOrEmpty(values.CreatedBy) ? loggedUser?.User?.Id : values.CreatedBy;

            var response = await dictRepository.SaveAsync(values);
            return mapper.Map<ValueRegistryDto>(response);
        }

        public async Task<ValueRegistryDto> CreateAsync(ValueRegistryDto valuesDto, LoggedUserDto? loggedUser)
        {
            if (!valuesDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var values = mapper.Map<ValueRegistry>(valuesDto);
            ValidatePermissions(values, loggedUser?.User);
            values.Id = string.Empty;
            values.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await dictRepository.SaveAsync(values, loggedUser.User.Id);
            return mapper.Map<ValueRegistryDto>(response);
        }

        public async Task<ValueRegistryDto?> UpdateAsync(ValueRegistryDto valuesDto, LoggedUserDto? loggedUser)
        {
            if (!valuesDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var values = mapper.Map<ValueRegistry>(valuesDto);
            ValidatePermissions(values, loggedUser?.User);
            var currentValues = await dictRepository.GetByIdAsync(values.Id);
            if (currentValues == null || currentValues.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await dictRepository.SaveAsync(values, loggedUser.User.Id);
            return mapper.Map<ValueRegistryDto>(response);
        }

        public async Task<bool> DeleteAsync(string valueId, LoggedUserDto? loggedUser)
        {
            var value = await dictRepository.GetByIdAsync(valueId);
            if (value == null)
            {
                return false;
            }
            ValidatePermissions(value, loggedUser?.User);
            await dictRepository.DeleteAsync(valueId);
            return true;
        }

        private void ValidatePermissions(ValueRegistry values, UserDto? loggedUser)
        {
            if (loggedUser != null && values.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Values/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    values.Id, values.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}