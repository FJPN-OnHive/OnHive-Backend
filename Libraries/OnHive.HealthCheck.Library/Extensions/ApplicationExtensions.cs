using OnHive.HealthCheck.Library.Abstractions;
using OnHive.HealthCheck.Library.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OnHive.HealthCheck.Library.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddHealthChecks(this WebApplication app)
        {
            app.MapGet("hc/readiness", async ([FromServices] IHealthCheckCollection healthcheckCollection) =>
            {
                var result = new List<HealthCheckResult>();
                foreach (var healthcheck in healthcheckCollection.HealthChecks)
                {
                    result.Add(await healthcheck.CheckHealthAsync(new HealthCheckContext()));
                }
                if (result.All(h => h.Status == HealthStatus.Healthy))
                {
                    var doc = HealthCheckHelpers.SerializerResultToDocument(result);
                    return Results.Ok(doc);
                }
                else
                {
                    return Results.Problem(HealthCheckHelpers.SerializerResult(result), title: "Unhealthy", type: "HealthCheck");
                }
            })
            .WithName("readiness")
            .WithTags("HealthCheck")
            .AllowAnonymous()
            .WithOpenApi();

            app.MapGet("hc/liveness", () =>
            {
                return Results.Ok<string>("Ok");
            })
            .WithName("liveness")
            .WithTags("HealthCheck")
            .AllowAnonymous()
            .WithOpenApi();

            return app;
        }
    }
}