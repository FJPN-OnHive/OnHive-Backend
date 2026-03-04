using OnHive.Core.Library.Abstractions.Enrich;
using OnHive.Core.Library.Entities.Enrichments;
using OnHive.Database.Library.Models;
using OnHive.Enrich.Library.Repository;
using Microsoft.Extensions.DependencyInjection;
using OnHive.Domains.Common.Helpers;
using System.Text.Json;

namespace OnHive.Enrich.Library.Extensions
{
    public static class EnrichExtension
    {
        public static async Task<T> LoadEnrichmentAsync<T>(this T enrichable) where T : IEnrichable
        {
            var repository = GetRepository();
            var enrichEntity = await repository.GetEnrich(enrichable.TenantId, enrichable.Id, enrichable.GetType().Name);
            if (enrichEntity != null)
            {
                enrichable.CustomAttributes = JsonSerializer.Deserialize<Dictionary<string, object>>(enrichEntity.CustomAttributes) ?? new Dictionary<string, object>();
            }
            else
            {
                enrichable.CustomAttributes = new Dictionary<string, object>();
            }
            return enrichable;
        }

        public static async Task<T> SaveEnrichmentAsync<T>(this T enrichable) where T : IEnrichable
        {
            var repository = GetRepository();
            var enrichEntity = await repository.GetEnrich(enrichable.TenantId, enrichable.Id, enrichable.GetType().Name);
            if (enrichEntity == null)
            {
                enrichEntity = new Enrichment
                {
                    TenantId = enrichable.TenantId,
                    EntityId = enrichable.Id,
                    EntityType = enrichable.GetType().Name,
                    CustomAttributes = JsonSerializer.Serialize(enrichable.CustomAttributes)
                };
                await repository.SaveAsync(enrichEntity);
            }
            else
            {
                enrichEntity.CustomAttributes = JsonSerializer.Serialize(enrichable.CustomAttributes);
                await repository.SaveAsync(enrichEntity);
            }
            return enrichable;
        }

        private static EnrichRepository GetRepository()
        {
            var serviceProvider = ServiceProviderFactory.ServiceProvider;
            var dbSettings = serviceProvider.GetRequiredService<MongoDBSettings>();
            return new EnrichRepository(dbSettings);
        }
    }
}