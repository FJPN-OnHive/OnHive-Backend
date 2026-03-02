using AutoMapper;
using EHive.Configuration.Library.Exceptions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.SystemParameters;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Entities.SystemParameters;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.SystemParameters.Domain.Abstractions.Repositories;
using EHive.SystemParameters.Domain.Abstractions.Services;
using EHive.SystemParameters.Domain.Models;
using Serilog;
using System.Text.Json;

namespace EHive.SystemParameters.Services
{
    public class SystemParametersService : ISystemParametersService
    {
        private readonly ISystemParametersRepository systemParametersRepository;
        private readonly SystemParametersApiSettings systemParametersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public SystemParametersService(ISystemParametersRepository systemParametersRepository, SystemParametersApiSettings systemParametersApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.systemParametersRepository = systemParametersRepository;
            this.systemParametersApiSettings = systemParametersApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<SystemParameterDto?> GetByIdAsync(string systemParameterId)
        {
            var systemParameter = await systemParametersRepository.GetByIdAsync(systemParameterId);
            return mapper.Map<SystemParameterDto>(systemParameter);
        }

        public async Task<IEnumerable<SystemParameterDto>> GetByGroupAsync(string group)
        {
            var systemParameter = await systemParametersRepository.GetByGroupAsync(group);
            return mapper.Map<IEnumerable<SystemParameterDto>>(systemParameter);
        }

        public async Task<PaginatedResult<SystemParameterDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await systemParametersRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<SystemParameterDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<SystemParameterDto>>(result.Itens)
                };
            }
            return new PaginatedResult<SystemParameterDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<SystemParameterDto>()
            };
        }

        public async Task<IEnumerable<SystemParameterDto>> GetAllAsync()
        {
            var systemParameters = await systemParametersRepository.GetAllAsync();
            return mapper.Map<IEnumerable<SystemParameterDto>>(systemParameters);
        }

        public async Task<SystemParameterDto> SaveAsync(SystemParameterDto systemParameterDto, UserDto? loggedUser)
        {
            var systemParameter = mapper.Map<SystemParameter>(systemParameterDto);
            ValidatePermissions(systemParameter, loggedUser);
            systemParameter.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            systemParameter.CreatedAt = DateTime.UtcNow;
            systemParameter.CreatedBy = string.IsNullOrEmpty(systemParameter.CreatedBy) ? loggedUser.Id : systemParameter.CreatedBy;

            var response = await systemParametersRepository.SaveAsync(systemParameter);
            return mapper.Map<SystemParameterDto>(response);
        }

        public async Task<SystemParameterDto> CreateAsync(SystemParameterDto systemParameterDto, UserDto loggedUser)
        {
            var systemParameter = mapper.Map<SystemParameter>(systemParameterDto);
            ValidatePermissions(systemParameter, loggedUser);
            systemParameter.Id = string.Empty;
            systemParameter.TenantId = loggedUser.TenantId;
            var response = await systemParametersRepository.SaveAsync(systemParameter, loggedUser.Id);
            return mapper.Map<SystemParameterDto>(response);
        }

        public async Task<SystemParameterDto?> UpdateAsync(SystemParameterDto systemParameterDto, UserDto loggedUser)
        {
            var systemParameter = mapper.Map<SystemParameter>(systemParameterDto);
            ValidatePermissions(systemParameter, loggedUser);
            var currentSystemParameter = await systemParametersRepository.GetByIdAsync(systemParameter.Id);
            if (currentSystemParameter == null || currentSystemParameter.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            var response = await systemParametersRepository.SaveAsync(systemParameter, loggedUser.Id);
            return mapper.Map<SystemParameterDto>(response);
        }

        public async Task<SystemParameterDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentSystemParameter = await systemParametersRepository.GetByIdAsync(patch.GetId());
            if (currentSystemParameter == null || currentSystemParameter.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentSystemParameter = patch.PatchEntity(currentSystemParameter);
            if (!mapper.Map<SystemParameterDto>(currentSystemParameter).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentSystemParameter, loggedUser);
            var response = await systemParametersRepository.SaveAsync(currentSystemParameter, loggedUser.Id);
            return mapper.Map<SystemParameterDto>(response);
        }

        public async Task Migrate()
        {
            var currents = await systemParametersRepository.GetAllAsync();
            var parameters = GetMigrationFile();
            foreach (var parameter in parameters)
            {
                if (!currents.Any(p => p.Id.Equals(parameter.Id, StringComparison.InvariantCultureIgnoreCase)))
                {
                    await systemParametersRepository.SaveAsync(parameter);
                }
            }
        }

        private List<SystemParameter> GetMigrationFile()
        {
            var file = JsonSerializer.Deserialize<List<SystemParameter>>(File.ReadAllText(Path.Join("Migrations", "initialValues.json")));
            if (file == null)
            {
                logger.Error("Missing initial values file.");
                throw new MissingConfigurationException<List<SystemParameter>>("initialValues.json");
            }
            return file;
        }

        private void ValidatePermissions(SystemParameter systemParameter, UserDto? loggedUser)
        {
            if (loggedUser != null
                && !loggedUser.Permissions.Exists(p => p.Equals(systemParametersApiSettings.SystemParametersAdminPermission, StringComparison.InvariantCultureIgnoreCase)))
            {
                logger.Warning("Unauthorized update: {id}, logged user: {loggedUserId}", systemParameter, loggedUser.Id);
                throw new UnauthorizedAccessException();
            }
        }
    }
}