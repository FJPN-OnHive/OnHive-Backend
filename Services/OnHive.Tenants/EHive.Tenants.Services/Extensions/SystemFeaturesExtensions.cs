using EHive.Core.Library.Entities.Tenants;
using EHive.Core.Library.Extensions;
using System.Text.Json;

namespace EHive.Tenants.Services.Extensions
{
    public static class SystemFeaturesExtensions
    {
        public static SystemFeatures SetHash(this SystemFeatures features)
        {
            var payload = JsonSerializer.Serialize(features.Features);
            features.Hash = payload.HashMd5();
            return features;
        }
    }
}