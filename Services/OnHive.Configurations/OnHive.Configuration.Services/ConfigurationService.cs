using AutoMapper;
using OnHive.Configuration.Domain.Abstractions.Repositories;
using OnHive.Configuration.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Configuration;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Configuration;
using Serilog;

namespace OnHive.Configuration.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository configsRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ConfigurationService(IConfigurationRepository configsRepository, IMapper mapper)
        {
            this.configsRepository = configsRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<IEnumerable<ConfigItemDto>> GetAllAsync(UserDto? loggedUser)
        {
            var config = await configsRepository.GetAllAsync(string.Empty);
            return mapper.Map<IEnumerable<ConfigItemDto>>(config);
        }

        public async Task<PaginatedResult<ConfigItemDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await configsRepository.GetByFilterAsync(filter, string.Empty, false);
            if (result != null)
            {
                return new PaginatedResult<ConfigItemDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<ConfigItemDto>>(result.Itens)
                };
            }
            return new PaginatedResult<ConfigItemDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<ConfigItemDto>()
            };
        }

        public async Task<ConfigItemDto?> GetByIdAsync(string id, UserDto? loggedUser)
        {
            var configItem = await configsRepository.GetByIdAsync(id);
            return mapper.Map<ConfigItemDto>(configItem);
        }

        public async Task<ConfigItemDto?> GetByKeyAsync(string key, UserDto? loggedUser)
        {
            var configItem = await configsRepository.GetByKeyAsync(key);
            return mapper.Map<ConfigItemDto>(configItem);
        }

        public async Task<ConfigItemDto?> GetByTypeAsync<T>()
        {
            var key = typeof(T).Name;
            var configItem = await configsRepository.GetByKeyAsync(key);
            return mapper.Map<ConfigItemDto>(configItem);
        }

        public async Task<ConfigItemDto> SaveAsync(ConfigItemDto configDto, UserDto? loggedUser)
        {
            var config = mapper.Map<ConfigItem>(configDto);
            ValidatePermissions(config, loggedUser);
            config.TenantId = "";
            config.CreatedAt = DateTime.UtcNow;
            config.CreatedBy = string.IsNullOrEmpty(config.CreatedBy) ? loggedUser?.Id ?? string.Empty : config.CreatedBy;
            var response = await configsRepository.SaveAsync(config);
            return mapper.Map<ConfigItemDto>(response);
        }

        private void ValidatePermissions(ConfigItem configItem, UserDto? loggedUser)
        {
            if (loggedUser != null && !loggedUser.Permissions.Contains("admin"))
            {
                logger.Warning("Unauthorized not a system admin Config: {id}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    configItem.Id, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}