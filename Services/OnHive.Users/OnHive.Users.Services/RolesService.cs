using AutoMapper;
using OnHive.Configuration.Library.Exceptions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Helpers;
using OnHive.Users.Domain.Abstractions.Repositories;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Users.Domain.Models;
using Serilog;
using System.Text.Json;

namespace OnHive.Users.Services
{
    public class RolesService : IRolesService
    {
        private readonly IRolesRepository rolesRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly HttpClient httpClient;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public RolesService(IRolesRepository rolesRepository, UsersApiSettings usersApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.rolesRepository = rolesRepository;
            this.usersApiSettings = usersApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
            this.httpClient = httpClient;
        }

        public async Task<RoleDto?> GetByIdAsync(string roleId)
        {
            var role = await rolesRepository.GetByIdAsync(roleId);
            return mapper.Map<RoleDto>(role);
        }

        public async Task<PaginatedResult<RoleDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await rolesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<RoleDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<RoleDto>>(result.Itens)
                };
            }
            return new PaginatedResult<RoleDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<RoleDto>()
            };
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var roles = await rolesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> SaveAsync(RoleDto roleDto, LoggedUserDto? loggedUser)
        {
            var role = mapper.Map<Role>(roleDto);
            ValidatePermissions(role, loggedUser?.User);
            await ValidadeDuplicate(role, loggedUser?.User?.TenantId);
            role.TenantId = loggedUser.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            role.CreatedAt = DateTime.UtcNow;
            role.CreatedBy = string.IsNullOrEmpty(role.CreatedBy) ? loggedUser.User?.Id : role.CreatedBy;

            var response = await rolesRepository.SaveAsync(role);
            return mapper.Map<RoleDto>(response);
        }

        public async Task<RoleDto> CreateAsync(RoleDto roleDto, LoggedUserDto loggedUser)
        {
            var role = mapper.Map<Role>(roleDto);
            ValidatePermissions(role, loggedUser.User);
            await ValidadeDuplicate(role, loggedUser?.User?.TenantId);
            role.Id = string.Empty;
            role.TenantId = loggedUser.User?.TenantId;
            var response = await rolesRepository.SaveAsync(role, loggedUser.User?.Id);
            return mapper.Map<RoleDto>(response);
        }

        public async Task<RoleDto?> UpdateAsync(RoleDto roleDto, LoggedUserDto loggedUser)
        {
            var role = mapper.Map<Role>(roleDto);
            ValidatePermissions(role, loggedUser.User);
            await ValidadeDuplicate(role, loggedUser.User?.TenantId);
            var currentRole = await rolesRepository.GetByIdAsync(role.Id);
            if (currentRole == null || currentRole.TenantId != loggedUser.User?.TenantId)
            {
                return null;
            }
            var response = await rolesRepository.SaveAsync(role, loggedUser.User?.Id);
            return mapper.Map<RoleDto>(response);
        }

        public async Task<RoleDto?> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentRole = await rolesRepository.GetByIdAsync(patch.GetId());
            if (currentRole == null || currentRole.TenantId != loggedUser.User?.TenantId)
            {
                return null;
            }
            currentRole = patch.PatchEntity(currentRole);
            ValidatePermissions(currentRole, loggedUser.User);
            var response = await rolesRepository.SaveAsync(currentRole, loggedUser.User?.Id);
            return mapper.Map<RoleDto>(response);
        }

        public async Task Migrate(bool isProduction)
        {
            var file = GetMigrationFile(isProduction);
            foreach (var role in file)
            {
                var current = await rolesRepository.GetByNameAsync(role.Name, role.TenantId);
                if (current == null)
                {
                    await rolesRepository.SaveAsync(role);
                }
            }
        }

        public async Task<List<string>> GetPermissions()
        {
            return PermissionsStore.Permissions.Select(p => p.Permission).Distinct().ToList();
        }

        private async Task ValidadeDuplicate(Role role, string? tenantId)
        {
            var currentRole = await rolesRepository.GetByNameAsync(role.Name, tenantId);
            if (currentRole != null && currentRole.Id != role.Id)
            {
                throw new DuplicatedException(role.Name);
            }
        }

        private void ValidatePermissions(Role role, UserDto? loggedUser)
        {
            if (loggedUser != null
                && !loggedUser.Permissions.Exists(p => p.Equals(usersApiSettings.UserAdminPermission, StringComparison.InvariantCultureIgnoreCase)))
            {
                logger.Warning("Unauthorized update: {id}, logged user: {loggedUserId}", role, loggedUser.Id);
                throw new UnauthorizedAccessException();
            }

            if (loggedUser != null && role.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Role/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    role.Id, role.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private List<Role> GetMigrationFile(bool isProduction)
        {
            var environment = isProduction ? "prod" : "staging";
            var file = JsonSerializer.Deserialize<List<Role>>(File.ReadAllText(Path.Join("Migrations", $"defaultRoles.{environment}.json")));
            if (file == null)
            {
                logger.Error("Missing features migration file.");
                throw new MissingConfigurationException<List<Role>>($"defaultRoles.{environment}.json");
            }
            return file;
        }
    }
}