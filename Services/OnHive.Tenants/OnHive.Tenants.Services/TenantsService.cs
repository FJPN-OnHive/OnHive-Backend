using AutoMapper;
using OnHive.Configuration.Library.Exceptions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Domains.Authorization.Models;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Models;
using OnHive.Users.Domain.Abstractions.Services;
using Serilog;
using System.Text.Json;

namespace OnHive.Tenants.Services
{
    public class TenantsService : ITenantsService
    {
        private readonly ITenantsRepository tenantsRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IFeaturesRepository featuresRepository;
        private readonly IUsersService usersService;
        private readonly IRolesService rolesService;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public TenantsService(ITenantsRepository tenantsRepository,
                              TenantsApiSettings tenantsApiSettings,
                              IFeaturesRepository featuresRepository,
                              IUsersService usersService,
                              IRolesService rolesService,
                              IMapper mapper)
        {
            this.tenantsRepository = tenantsRepository;
            this.tenantsApiSettings = tenantsApiSettings;
            this.featuresRepository = featuresRepository;
            this.usersService = usersService;
            this.rolesService = rolesService;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<TenantDto> SetupTenantAsync(TenantSetupDto tenantDto)
        {
            if (!tenantDto.Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var currentTenants = await tenantsRepository.GetAllAsync();
            if (currentTenants.Any())
            {
                throw new UnauthorizedAccessException();
            }
            
            var tenant = new Tenant
            {
                Id = tenantDto.Id,
                TenantId = tenantDto.Id,
                Name = tenantDto.Name ?? string.Empty,
                Domain = tenantDto.Domain ?? string.Empty,
                Email = tenantDto.Email ?? string.Empty,
                CNPJ = tenantDto.CNPJ ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "System",
                Version = "1",
                VersionNumber = 1
            };

            var user = new SignInUserDto
            {
                TenantId = tenantDto.Id,
                Name = tenantDto.AdminUserName!,
                Email = tenantDto.AdminUserEmail!,
                Password = tenantDto.AdminUserPassword!,
                Login = tenantDto.AdminUserName!,
                IsForeigner = false,
                Occupation = "admin",
                Gender = "NONE"
            };

            var roles = GetInitialRoles(tenant);

            await tenantsRepository.SaveAsync(tenant);
            await rolesService.CreateAsync(roles.Select(r => mapper.Map<RoleDto>(r)).ToList());
            await usersService.CreateWithRolesAsync(user, ["admin"]);

            return mapper.Map<TenantDto>(tenant);
        }

        public async Task<TenantDto?> GetByIdAsync(string tenantId)
        {
            var tenant = await tenantsRepository.GetByIdAsync(tenantId);
            return mapper.Map<TenantDto>(tenant);
        }

        public async Task<string> GetByDomainAsync(string subdomain)
        {
            var tenant = await tenantsRepository.GetByDomainAsync(subdomain);
            return tenant?.Id ?? string.Empty;
        }

        public async Task<List<TenantResumeDto>> GetAllOpenAsync()
        {
            var tenants = await tenantsRepository.GetAllAsync();
            return mapper.Map<List<TenantResumeDto>>(tenants);
        }

        public async Task<TenantResumeDto> GetBySlugAsync(string slug)
        {
            var tenant = await tenantsRepository.GetBySlugAsync(slug);
            return mapper.Map<TenantResumeDto>(tenant);
        }

        public async Task<PaginatedResult<TenantDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await tenantsRepository.GetByFilterAsync(filter, loggedUser?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<TenantDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<TenantDto>>(result.Itens)
                };
            }
            return new PaginatedResult<TenantDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<TenantDto>()
            };
        }

        public async Task<IEnumerable<TenantDto>> GetAllAsync(UserDto? loggedUser)
        {
            var tenants = await tenantsRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<TenantDto>>(tenants);
        }

        public async Task<TenantDto> CreateAsync(TenantDto tenantDto, UserDto loggedUser)
        {
            var tenant = mapper.Map<Tenant>(tenantDto);
            tenant.Id = tenantDto.Id;
            tenant.TenantId = tenantDto.Id;
            tenant.Features = await GetFeatures(tenantDto.Features);
            var response = await tenantsRepository.SaveAsync(tenant, loggedUser.Id);
            return mapper.Map<TenantDto>(response);
        }

        public async Task<TenantDto?> UpdateAsync(TenantDto tenantDto, UserDto loggedUser)
        {
            var tenant = mapper.Map<Tenant>(tenantDto);
            tenant.Features = await GetFeatures(tenantDto.Features);
            tenant.TenantId = tenantDto.Id;
            ValidatePermissions(tenant, loggedUser);
            var currentTenant = await tenantsRepository.GetByIdAsync(tenant.Id);
            if (currentTenant == null || currentTenant.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            var response = await tenantsRepository.SaveAsync(tenant, loggedUser.Id);
            return mapper.Map<TenantDto>(response);
        }

        private async Task<List<Feature>> GetFeatures(List<string> features)
        {
            var result = await featuresRepository.GetAllAsync(string.Empty);
            if (result != null && result.Any())
            {
                return result[0].Features.Where(f => features.Contains(f.Key)).ToList();
            }
            return [];
        }

        public async Task<TenantDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentStudent = await tenantsRepository.GetByIdAsync(patch.GetId());
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.TenantId)
            {
                return null;
            }
            currentStudent = patch.PatchEntity(currentStudent);
            ValidatePermissions(currentStudent, loggedUser);
            if (!mapper.Map<TenantDto?>(currentStudent).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await tenantsRepository.SaveAsync(currentStudent, loggedUser.Id);
            return mapper.Map<TenantDto>(response);
        }

        public async Task Migrate(bool isProduction)
        {
            var file = GetMigrationFile(isProduction);
            var current = await tenantsRepository.GetByIdAsync(file.Id);
            if (current == null)
            {
                await tenantsRepository.SaveAsync(file);
            }
        }

        private void ValidatePermissions(Tenant? tenant, UserDto? loggedUser)
        {
            if (loggedUser != null && tenant?.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Tenant/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    tenant?.Id, tenant?.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private Tenant GetMigrationFile(bool isProduction)
        {
            var environment = isProduction ? "prod" : "staging";
            var file = JsonSerializer.Deserialize<Tenant>(File.ReadAllText(Path.Join("Migrations", $"defaultTenant.{environment}.json")));
            logger.Error($"Migration file = defaultTenant.{environment}.json");
            if (file == null)
            {
                logger.Error("Missing features migration file.");
                throw new MissingConfigurationException<Tenant>($"defaultTenant.{environment}.json");
            }
            return file;
        }

        private List<RoleDto> GetInitialRoles(Tenant tenant)
        {
            var permissions = PermissionsStore.Permissions.Select(p => p.Permission).ToList();
            var adminPermissions = permissions;
            adminPermissions.Add(PermissionConsts.SystemAdmin);
            adminPermissions.Add(PermissionConsts.Admin);
            adminPermissions.Add(PermissionConsts.Staff);
            var staffPermissions = permissions;
            staffPermissions.Add(PermissionConsts.Staff);
            var studentPermissions = new List<string> { "lms_access",
                                                        "account_access",
                                                        "users_read",
                                                        "users_update",
                                                        "payments_read",
                                                        "payments_create",
                                                        "teachers_read",
                                                        "students_update",
                                                        "students_read",
                                                        "students_create",
                                                        "classes_read",
                                                        "courses_read",
                                                        "carts_update",
                                                        "carts_read",
                                                        "carts_create",
                                                        "carts_delete",
                                                        "orders_read",
                                                        "orders_create",
                                                        "orders_status",
                                                        "products_read",
                                                        "addresses_read",
                                                        "addresses_create",
                                                        "addresses_update",
                                                        "addresses_delete"};
            var teacherPermissions = new List<string> { "lms_access",
                                                        "account_access",
                                                        "users_read",
                                                        "users_update",
                                                        "payments_read",
                                                        "payments_create",
                                                        "teachers_update",
                                                        "teachers_read",
                                                        "students_read",
                                                        "classes_update",
                                                        "classes_read",
                                                        "courses_update",
                                                        "courses_read",
                                                        "carts_update",
                                                        "carts_read",
                                                        "carts_create",
                                                        "carts_delete",
                                                        "orders_read",
                                                        "orders_create",
                                                        "orders_status",
                                                        "products_read",
                                                        "addresses_read",
                                                        "addresses_create",
                                                        "addresses_update",
                                                        "addresses_delete"};
            return new List<RoleDto>
            {
                new RoleDto
                {
                    TenantId = tenant.Id,
                    Id = Guid.NewGuid().ToString(),
                    Name = "admin",
                    Permissions = adminPermissions,
                    IsAdmin = true
                },
                new RoleDto
                {
                    TenantId = tenant.Id,
                    Id = Guid.NewGuid().ToString(),
                    Name = "staff",
                    Permissions = staffPermissions
                },
                new RoleDto
                {
                    TenantId = tenant.Id,
                    Id = Guid.NewGuid().ToString(),
                    Name = "student",
                    Permissions = studentPermissions
                },
                new RoleDto
                {
                    TenantId = tenant.Id,
                    Id = Guid.NewGuid().ToString(),
                    Name = "teacher",
                    Permissions = teacherPermissions
                }
            };
        }


        
    }
}