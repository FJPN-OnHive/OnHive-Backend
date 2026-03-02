using AutoMapper;
using EHive.Configuration.Library.Exceptions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Entities.Tenants;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.Tenants.Domain.Abstractions.Repositories;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.Tenants.Domain.Models;
using Serilog;
using System.Text.Json;

namespace EHive.Tenants.Services
{
    public class TenantsService : ITenantsService
    {
        private readonly ITenantsRepository tenantsRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IFeaturesRepository featuresRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public TenantsService(ITenantsRepository tenantsRepository,
                              TenantsApiSettings tenantsApiSettings,
                              IFeaturesRepository featuresRepository,
                              IMapper mapper)
        {
            this.tenantsRepository = tenantsRepository;
            this.tenantsApiSettings = tenantsApiSettings;
            this.featuresRepository = featuresRepository;
            this.mapper = mapper;
            logger = Log.Logger;
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
    }
}