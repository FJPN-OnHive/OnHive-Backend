using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OnHive.HealthCheck.Library.Abstractions
{
    public interface IHealthCheckCollection
    {
        List<IHealthCheck> HealthChecks { get; }

        void AddHealthCheck(IHealthCheck healthCheck);
    }
}