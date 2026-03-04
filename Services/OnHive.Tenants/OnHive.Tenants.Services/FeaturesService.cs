using AutoMapper;
using OnHive.Configuration.Library.Exceptions;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Tenants.Services.Extensions;
using Serilog;
using System.Text.Json;

namespace OnHive.Tenants.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IFeaturesRepository featuresRepository;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public FeaturesService(IFeaturesRepository featuresRepository, IMapper mapper)
        {
            this.featuresRepository = featuresRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<IEnumerable<FeatureDto>> GetAllAsync()
        {
            var systemFeatures = await featuresRepository.GetAllAsync(string.Empty);
            if (systemFeatures == null || !systemFeatures.Any())
            {
                return [];
            }
            return mapper.Map<IEnumerable<FeatureDto>>(systemFeatures[0].Features);
        }

        public async Task Migrate()
        {
            var current = await GetCurrent();
            var file = GetMigrationFile();
            file.SetHash();
            if (!current.Hash.Equals(file.Hash, StringComparison.InvariantCultureIgnoreCase))
            {
                current.Features = file.Features;
                current.UpdatedAt = DateTime.UtcNow;
                current.SetHash();
                await featuresRepository.SaveAsync(current);
            }
        }

        private SystemFeatures GetMigrationFile()
        {
            var file = JsonSerializer.Deserialize<SystemFeatures>(File.ReadAllText(Path.Join("Migrations", "features.json")));
            if (file == null)
            {
                logger.Error("Missing features migration file.");
                throw new MissingConfigurationException<SystemFeatures>("features.json");
            }
            return file;
        }

        private async Task<SystemFeatures> GetCurrent()
        {
            var current = await featuresRepository.GetAsync();
            if (current == null)
            {
                current = new SystemFeatures
                {
                    CreatedBy = "0",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = "0",
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
            }
            return current;
        }
    }
}