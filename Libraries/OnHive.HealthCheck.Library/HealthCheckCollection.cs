using OnHive.HealthCheck.Library.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OnHive.HealthCheck.Library
{
    public class HealthCheckCollection : IHealthCheckCollection
    {
        private static List<IHealthCheck> healthChecks = new List<IHealthCheck>();

        public List<IHealthCheck> HealthChecks => healthChecks;

        public void AddHealthCheck(IHealthCheck healthCheck)
        {
            healthChecks.Add(healthCheck);
        }
    }
}