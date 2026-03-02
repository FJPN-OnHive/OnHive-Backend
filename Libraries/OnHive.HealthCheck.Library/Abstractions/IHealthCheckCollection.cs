using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHive.HealthCheck.Library.Abstractions
{
    public interface IHealthCheckCollection
    {
        List<IHealthCheck> HealthChecks { get; }

        void AddHealthCheck(IHealthCheck healthCheck);
    }
}