using EHive.Database.Library.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace EHive.Database.Library.HealthChecks
{
    internal class MongoDbHealthCheck : IHealthCheck
    {
        private MongoDBSettings? settings;

        public MongoDbHealthCheck(MongoDBSettings? settings)
        {
            this.settings = settings;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoClient = new MongoClient(settings?.ConnectionString);
                var database = mongoClient.GetDatabase(settings?.DataBase);
                var collections = await database.ListCollectionNamesAsync();

                return new HealthCheckResult(HealthStatus.Healthy, "MongoDb");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "MongoDb", ex);
            }
        }
    }
}